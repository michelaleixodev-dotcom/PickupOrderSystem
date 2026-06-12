using PickupOrderSystem.Application.DTOs;
using PickupOrderSystem.Application.Interfaces;
using PickupOrderSystem.Application.Interfaces.Repositories;
using PickupOrderSystem.Domain.Entities;
using PickupOrderSystem.Domain.Enums;
using PickupOrderSystem.Domain.Exceptions;
using Serilog;

namespace PickupOrderSystem.Application.Services;

public class PickupRequestService(
    IPickupRequestRepository pickupRequestRepo,
    IDriverRepository driverRepo,
    IVehicleRepository vehicleRepo,
    IUnitOfWork unitOfWork) : IPickupRequestService
{
    private static readonly Serilog.ILogger Logger = Log.ForContext<PickupRequestService>();

    public async Task<PagedResult<PickupRequestDto>> GetListAsync(Guid? userId, string? status = null, string? clientName = null, DateOnly? from = null, DateOnly? to = null, int page = 1, int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var total = await pickupRequestRepo.CountAsync(userId, status, clientName, from, to);
        var requests = await pickupRequestRepo.GetAllAsync(userId, status, clientName, from, to, page, pageSize);

        var items = requests
            .Select(r => new PickupRequestDto(
                r.Id, r.IdentificationNumber, r.User.Name,
                r.Sender, r.PickupAddress, r.Recipient, r.DeliveryAddress,
                r.RequestDate, r.ScheduledPickupDate,
                r.Priority.ToString(), r.Status.ToString(), r.Notes, null, null, null))
            .ToList();

        return new PagedResult<PickupRequestDto>(items, page, pageSize, total, (int)Math.Ceiling((double)total / pageSize));
    }

    public async Task<PickupRequestDto> GetByIdAsync(Guid id, Guid requestingUserId, string role)
    {
        var r = await pickupRequestRepo.GetByIdAsync(id)
            ?? throw new NotFoundException();

        if (role == "Cliente" && r.UserId != requestingUserId)
            throw new ForbiddenException();

        var active = r.Assignments.FirstOrDefault(a => a.ActualEndDate == null);
        var assignment = active is null ? null : new AssignmentDto(
            active.Id, active.Driver.Name,
            active.Vehicle.LicensePlate, active.Vehicle.Model,
            active.AssignmentDate);

        var statusHistory = r.StatusHistories
            .OrderBy(h => h.ChangedAt)
            .Select(h => new StatusHistoryDto(h.FromStatus, h.ToStatus, h.ChangedAt, h.ChangedByNameSnapshot))
            .ToList();

        var occurrences = r.Occurrences
            .OrderByDescending(o => o.OccurrenceDate)
            .Select(o => new OccurrenceDto(o.Id, o.Type.ToString(), o.Description, o.OccurrenceDate, o.RegisteredByNameSnapshot, o.Resolved, o.ResolutionNotes))
            .ToList();

        return new PickupRequestDto(
            r.Id, r.IdentificationNumber, r.User.Name,
            r.Sender, r.PickupAddress, r.Recipient, r.DeliveryAddress,
            r.RequestDate, r.ScheduledPickupDate,
            r.Priority.ToString(), r.Status.ToString(), r.Notes,
            assignment, statusHistory, occurrences);
    }

    public async Task<(Guid Id, string IdentificationNumber)> CreateAsync(
        CreatePickupRequestRequest body, Guid userId, string userName)
    {
        if (!Enum.TryParse<Priority>(body.Priority, out var priority))
        {
            Logger.Warning("[COLETA][CRIAR] Prioridade inválida recebida. Valor: {Prioridade}, UserId: {UserId}", body.Priority, userId);
            throw new BusinessRuleException("Prioridade inválida.");
        }

        var year = DateTime.UtcNow.Year;
        var prefix = $"COL-{year}-";
        var last = await pickupRequestRepo.GetLastIdentificationNumberAsync(prefix);
        var nextSeq = last is null ? 1 : int.Parse(last[prefix.Length..]) + 1;

        var now = DateTime.UtcNow;
        var request = new PickupRequest
        {
            Id = Guid.NewGuid(),
            IdentificationNumber = $"{prefix}{nextSeq:D4}",
            UserId = userId,
            Sender = body.Sender,
            PickupAddress = body.PickupAddress,
            Recipient = body.Recipient,
            DeliveryAddress = body.DeliveryAddress,
            RequestDate = now,
            ScheduledPickupDate = body.ScheduledPickupDate,
            Priority = priority,
            Status = PickupRequestStatus.Aberta,
            Notes = body.Notes,
            CreatedAt = now,
            UpdatedAt = now
        };

        pickupRequestRepo.Add(request);
        pickupRequestRepo.AddStatusHistory(new StatusHistory
        {
            Id = Guid.NewGuid(),
            PickupRequestId = request.Id,
            FromStatus = null,
            ToStatus = PickupRequestStatus.Aberta.ToString(),
            ChangedAt = now,
            ChangedById = userId,
            ChangedByNameSnapshot = userName
        });

        await unitOfWork.SaveChangesAsync();

        Logger.Information("[COLETA][CRIAR] Solicitação criada com sucesso. Número: {Numero}, Prioridade: {Prioridade}, CriadoPor: {Usuario} ({UserId})",
            request.IdentificationNumber, priority, userName, userId);

        return (request.Id, request.IdentificationNumber);
    }

    public async Task UpdateStatusAsync(Guid id, string status, Guid userId, string userName)
    {
        if (!Enum.TryParse<PickupRequestStatus>(status, out var newStatus))
        {
            Logger.Warning("[COLETA][STATUS] Status inválido recebido. Valor: {Status}, SolicitacaoId: {Id}, UserId: {UserId}", status, id, userId);
            throw new BusinessRuleException("Status inválido.");
        }

        var r = await pickupRequestRepo.GetByIdAsync(id)
            ?? throw new NotFoundException();

        var allowed = r.Status switch
        {
            PickupRequestStatus.Aberta            => new[] { PickupRequestStatus.Cancelada },
            PickupRequestStatus.Atribuida         => new[] { PickupRequestStatus.EmColeta, PickupRequestStatus.Cancelada },
            PickupRequestStatus.EmColeta          => new[] { PickupRequestStatus.Coletado, PickupRequestStatus.FalhaNaColeta, PickupRequestStatus.Cancelada },
            PickupRequestStatus.Coletado          => new[] { PickupRequestStatus.ACaminho, PickupRequestStatus.Cancelada },
            PickupRequestStatus.ACaminho          => new[] { PickupRequestStatus.Concluida, PickupRequestStatus.Cancelada },
            PickupRequestStatus.FalhaNaColeta     => new[] { PickupRequestStatus.AguardandoDecisao, PickupRequestStatus.Cancelada },
            PickupRequestStatus.AguardandoDecisao => new[] { PickupRequestStatus.Atribuida, PickupRequestStatus.Cancelada },
            _                                     => Array.Empty<PickupRequestStatus>()
        };

        if (!allowed.Contains(newStatus))
        {
            Logger.Warning("[COLETA][STATUS] Transição de status não permitida. Numero: {Numero}, De: {De}, Para: {Para}, Usuario: {Usuario}",
                r.IdentificationNumber, r.Status, newStatus, userName);
            throw new BusinessRuleException($"Transição de '{r.Status}' para '{newStatus}' não é permitida.");
        }

        var previousStatus = r.Status;
        var now = DateTime.UtcNow;

        pickupRequestRepo.AddStatusHistory(new StatusHistory
        {
            Id = Guid.NewGuid(),
            PickupRequestId = id,
            FromStatus = r.Status.ToString(),
            ToStatus = newStatus.ToString(),
            ChangedAt = now,
            ChangedById = userId,
            ChangedByNameSnapshot = userName
        });

        r.Status = newStatus;
        r.UpdatedAt = now;

        await unitOfWork.SaveChangesAsync();

        Logger.Information("[COLETA][STATUS] Transição realizada. Numero: {Numero}, De: {De}, Para: {Para}, AlteradoPor: {Usuario}",
            r.IdentificationNumber, previousStatus, newStatus, userName);
    }

    public async Task AssignAsync(Guid id, AssignRequest body, Guid userId, string userName)
    {
        var r = await pickupRequestRepo.GetByIdAsync(id)
            ?? throw new NotFoundException();

        var assignable = new[] { PickupRequestStatus.Aberta, PickupRequestStatus.AguardandoDecisao };
        if (!assignable.Contains(r.Status))
        {
            Logger.Warning("[COLETA][ATRIBUIR] Status não permite atribuição. Numero: {Numero}, Status: {Status}, Usuario: {Usuario}",
                r.IdentificationNumber, r.Status, userName);
            throw new BusinessRuleException("Apenas solicitações Abertas ou Aguardando Decisão podem ser atribuídas.");
        }

        var driver = await driverRepo.GetActiveByIdAsync(body.DriverId)
            ?? throw new BusinessRuleException("Motorista inválido.");

        var vehicle = await vehicleRepo.GetActiveByIdAsync(body.VehicleId)
            ?? throw new BusinessRuleException("Veículo inválido.");

        var now = DateTime.UtcNow;

        var activeAssignment = r.Assignments.FirstOrDefault(a => a.ActualEndDate == null);
        if (activeAssignment is not null)
        {
            Logger.Information("[COLETA][ATRIBUIR] Atribuição anterior encerrada. Numero: {Numero}, MotoristaPrevio: {MotoristaPrevioId}",
                r.IdentificationNumber, activeAssignment.DriverId);
            activeAssignment.ActualEndDate = now;
            activeAssignment.UpdatedAt = now;
        }

        pickupRequestRepo.AddAssignment(new Assignment
        {
            Id = Guid.NewGuid(),
            PickupRequestId = id,
            DriverId = body.DriverId,
            VehicleId = body.VehicleId,
            AssignmentDate = now,
            CreatedAt = now,
            UpdatedAt = now
        });

        pickupRequestRepo.AddStatusHistory(new StatusHistory
        {
            Id = Guid.NewGuid(),
            PickupRequestId = id,
            FromStatus = r.Status.ToString(),
            ToStatus = PickupRequestStatus.Atribuida.ToString(),
            ChangedAt = now,
            ChangedById = userId,
            ChangedByNameSnapshot = userName
        });

        r.Status = PickupRequestStatus.Atribuida;
        r.UpdatedAt = now;

        await unitOfWork.SaveChangesAsync();

        Logger.Information("[COLETA][ATRIBUIR] Motorista atribuído. Numero: {Numero}, Motorista: {Motorista} ({MotoristaId}), Veiculo: {Veiculo} ({VeiculoId}), AtribuidoPor: {Usuario}",
            r.IdentificationNumber, driver.Name, body.DriverId, vehicle.LicensePlate, body.VehicleId, userName);
    }

    public async Task RegisterOccurrenceAsync(Guid id, CreateOccurrenceRequest body, Guid userId, string userName)
    {
        if (!Enum.TryParse<OccurrenceType>(body.Type, out var type))
        {
            Logger.Warning("[COLETA][OCORRENCIA] Tipo de ocorrência inválido. Valor: {Tipo}, SolicitacaoId: {Id}, UserId: {UserId}", body.Type, id, userId);
            throw new BusinessRuleException("Tipo de ocorrência inválido.");
        }

        var r = await pickupRequestRepo.GetByIdAsync(id)
            ?? throw new NotFoundException();

        var now = DateTime.UtcNow;

        pickupRequestRepo.AddOccurrence(new Occurrence
        {
            Id = Guid.NewGuid(),
            PickupRequestId = id,
            Type = type,
            Description = body.Description,
            OccurrenceDate = now,
            RegisteredById = userId,
            RegisteredByNameSnapshot = userName,
            Resolved = false,
            CreatedAt = now,
            UpdatedAt = now
        });

        await unitOfWork.SaveChangesAsync();

        Logger.Information("[COLETA][OCORRENCIA] Ocorrência registrada. Numero: {Numero}, Tipo: {Tipo}, RegistradoPor: {Usuario}",
            r.IdentificationNumber, type, userName);
    }

    public async Task RegisterFailureAsync(Guid id, RegisterFailureRequest body, Guid userId, string userName)
    {
        if (!Enum.TryParse<OccurrenceType>(body.Type, out var type))
        {
            Logger.Warning("[COLETA][FALHA] Tipo de ocorrência inválido. Valor: {Tipo}, SolicitacaoId: {Id}, UserId: {UserId}", body.Type, id, userId);
            throw new BusinessRuleException("Tipo de ocorrência inválido.");
        }

        var r = await pickupRequestRepo.GetByIdAsync(id)
            ?? throw new NotFoundException();

        var failableStatuses = new[] { PickupRequestStatus.EmColeta, PickupRequestStatus.Coletado, PickupRequestStatus.ACaminho };
        if (!failableStatuses.Contains(r.Status))
        {
            Logger.Warning("[COLETA][FALHA] Registro de falha não permitido no status atual. Numero: {Numero}, Status: {Status}, Usuario: {Usuario}",
                r.IdentificationNumber, r.Status, userName);
            throw new BusinessRuleException("Falha só pode ser registrada nos status Em Coleta, Coletado ou A Caminho.");
        }

        var previousStatus = r.Status;
        var now = DateTime.UtcNow;

        pickupRequestRepo.AddOccurrence(new Occurrence
        {
            Id = Guid.NewGuid(),
            PickupRequestId = id,
            Type = type,
            Description = body.Description,
            OccurrenceDate = now,
            RegisteredById = userId,
            RegisteredByNameSnapshot = userName,
            Resolved = false,
            CreatedAt = now,
            UpdatedAt = now
        });

        pickupRequestRepo.AddStatusHistory(new StatusHistory
        {
            Id = Guid.NewGuid(),
            PickupRequestId = id,
            FromStatus = r.Status.ToString(),
            ToStatus = PickupRequestStatus.FalhaNaColeta.ToString(),
            ChangedAt = now,
            ChangedById = userId,
            ChangedByNameSnapshot = userName
        });

        r.Status = PickupRequestStatus.FalhaNaColeta;
        r.UpdatedAt = now;

        await unitOfWork.SaveChangesAsync();

        Logger.Warning("[COLETA][FALHA] Falha registrada na solicitação. Numero: {Numero}, StatusAnterior: {StatusAnterior}, Tipo: {Tipo}, RegistradoPor: {Usuario}",
            r.IdentificationNumber, previousStatus, type, userName);
    }
}
