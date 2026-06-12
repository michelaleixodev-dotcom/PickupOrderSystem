using PickupOrderSystem.Application.DTOs;
using PickupOrderSystem.Application.Interfaces;
using PickupOrderSystem.Application.Interfaces.Repositories;
using PickupOrderSystem.Domain.Entities;
using PickupOrderSystem.Domain.Enums;
using PickupOrderSystem.Domain.Exceptions;

namespace PickupOrderSystem.Application.Services;

public class PickupRequestService(
    IPickupRequestRepository pickupRequestRepo,
    IDriverRepository driverRepo,
    IVehicleRepository vehicleRepo,
    IUnitOfWork unitOfWork) : IPickupRequestService
{
    public async Task<IReadOnlyList<PickupRequestDto>> GetListAsync(Guid? userId)
    {
        var requests = await pickupRequestRepo.GetAllAsync(userId);

        return requests
            .Select(r => new PickupRequestDto(
                r.Id, r.IdentificationNumber, r.User.Name,
                r.Sender, r.PickupAddress, r.Recipient, r.DeliveryAddress,
                r.RequestDate, r.ScheduledPickupDate,
                r.Priority.ToString(), r.Status.ToString(), r.Notes, null, null, null))
            .ToList();
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
            throw new BusinessRuleException("Prioridade inválida.");

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

        return (request.Id, request.IdentificationNumber);
    }

    public async Task UpdateStatusAsync(Guid id, string status, Guid userId, string userName)
    {
        if (!Enum.TryParse<PickupRequestStatus>(status, out var newStatus))
            throw new BusinessRuleException("Status inválido.");

        var r = await pickupRequestRepo.GetByIdAsync(id)
            ?? throw new NotFoundException();

        var allowed = r.Status switch
        {
            PickupRequestStatus.Aberta            => new[] { PickupRequestStatus.Cancelada },
            PickupRequestStatus.Atribuida         => new[] { PickupRequestStatus.EmAndamento, PickupRequestStatus.Cancelada },
            PickupRequestStatus.EmAndamento       => new[] { PickupRequestStatus.Concluida, PickupRequestStatus.FalhaNaColeta, PickupRequestStatus.Cancelada },
            PickupRequestStatus.FalhaNaColeta     => new[] { PickupRequestStatus.AguardandoDecisao, PickupRequestStatus.Cancelada },
            PickupRequestStatus.AguardandoDecisao => new[] { PickupRequestStatus.Atribuida, PickupRequestStatus.Cancelada },
            _                                     => Array.Empty<PickupRequestStatus>()
        };

        if (!allowed.Contains(newStatus))
            throw new BusinessRuleException($"Transição de '{r.Status}' para '{newStatus}' não é permitida.");

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
    }

    public async Task AssignAsync(Guid id, AssignRequest body, Guid userId, string userName)
    {
        var r = await pickupRequestRepo.GetByIdAsync(id)
            ?? throw new NotFoundException();

        var assignable = new[] { PickupRequestStatus.Aberta, PickupRequestStatus.AguardandoDecisao };
        if (!assignable.Contains(r.Status))
            throw new BusinessRuleException("Apenas solicitações Abertas ou Aguardando Decisão podem ser atribuídas.");

        _ = await driverRepo.GetActiveByIdAsync(body.DriverId)
            ?? throw new BusinessRuleException("Motorista inválido.");

        _ = await vehicleRepo.GetActiveByIdAsync(body.VehicleId)
            ?? throw new BusinessRuleException("Veículo inválido.");

        var now = DateTime.UtcNow;

        var activeAssignment = r.Assignments.FirstOrDefault(a => a.ActualEndDate == null);
        if (activeAssignment is not null)
        {
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
    }

    public async Task RegisterOccurrenceAsync(Guid id, CreateOccurrenceRequest body, Guid userId, string userName)
    {
        if (!Enum.TryParse<OccurrenceType>(body.Type, out var type))
            throw new BusinessRuleException("Tipo de ocorrência inválido.");

        _ = await pickupRequestRepo.GetByIdAsync(id)
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
    }

    public async Task RegisterFailureAsync(Guid id, RegisterFailureRequest body, Guid userId, string userName)
    {
        if (!Enum.TryParse<OccurrenceType>(body.Type, out var type))
            throw new BusinessRuleException("Tipo de ocorrência inválido.");

        var r = await pickupRequestRepo.GetByIdAsync(id)
            ?? throw new NotFoundException();

        if (r.Status != PickupRequestStatus.EmAndamento)
            throw new BusinessRuleException("Apenas pedidos Em Andamento podem registrar falha.");

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
    }
}
