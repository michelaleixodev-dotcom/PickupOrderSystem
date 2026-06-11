using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PickupOrderSystem.Domain.Entities;
using PickupOrderSystem.Domain.Enums;

namespace PickupOrderSystem.Infrastructure.Data;

public static class DataSeeder
{
    private static string HashPassword(string password) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));

    // Placeholder que não bate com nenhum hash real — força reset de senha no 1º login
    private const string PlaceholderPasswordHash = "PLACEHOLDER_FORCE_RESET";

    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        // ── Colaboradores ─────────────────────────────────────────────────────
        var colaborador1 = new User
        {
            Id = Guid.Parse("11111111-0000-0000-0000-000000000001"),
            Name = "Lucas Mendes",
            Email = "lucas.mendes@pickupsystem.com",
            PasswordHash = HashPassword("Senha@123"),
            Role = UserRole.Colaborador,
            Active = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var colaborador2 = new User
        {
            Id = Guid.Parse("11111111-0000-0000-0000-000000000002"),
            Name = "Juliana Ramos",
            Email = "juliana.ramos@pickupsystem.com",
            PasswordHash = HashPassword("Senha@123"),
            Role = UserRole.Colaborador,
            Active = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        // ── Clientes ──────────────────────────────────────────────────────────
        var clienteDistrib = new User
        {
            Id = Guid.Parse("11111111-0000-0000-0000-000000000003"),
            Name = "Distribuidora Noroeste Ltda",
            Email = "contato@distribnoroeste.com.br",
            PasswordHash = HashPassword("Senha@123"),
            Role = UserRole.Cliente,
            Active = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var clienteFarma = new User
        {
            Id = Guid.Parse("11111111-0000-0000-0000-000000000004"),
            Name = "Farmacêutica Vida Nova Ltda",
            Email = "compras@vidanova.com.br",
            PasswordHash = HashPassword("Senha@123"),
            Role = UserRole.Cliente,
            Active = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var clienteMetalurgica = new User
        {
            Id = Guid.Parse("11111111-0000-0000-0000-000000000005"),
            Name = "Indústria Metalúrgica BR S.A.",
            Email = "logistica@metbr.com.br",
            PasswordHash = HashPassword("Senha@123"),
            Role = UserRole.Cliente,
            Active = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var clienteSuper = new User
        {
            Id = Guid.Parse("11111111-0000-0000-0000-000000000006"),
            Name = "Supermercados Central Ltda",
            Email = "abastecimento@supercentral.com.br",
            PasswordHash = HashPassword("Senha@123"),
            Role = UserRole.Cliente,
            Active = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        // ── Motoristas (migrados de drivers) ──────────────────────────────────
        var motoristaCarlos = new User
        {
            Id = Guid.Parse("77777777-0000-0000-0000-000000000001"),
            Name = "Carlos Eduardo Oliveira",
            Email = "carlos.oliveira@transportes.com",
            PasswordHash = PlaceholderPasswordHash,
            Role = UserRole.Motorista,
            Active = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var motoristaAna = new User
        {
            Id = Guid.Parse("77777777-0000-0000-0000-000000000002"),
            Name = "Ana Paula Santos",
            Email = "ana.santos@transportes.com",
            PasswordHash = PlaceholderPasswordHash,
            Role = UserRole.Motorista,
            Active = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var motoristaRoberto = new User
        {
            Id = Guid.Parse("77777777-0000-0000-0000-000000000003"),
            Name = "Roberto Ferreira Silva",
            Email = "roberto.silva@transportes.com",
            PasswordHash = PlaceholderPasswordHash,
            Role = UserRole.Motorista,
            Active = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var motoristaFernanda = new User
        {
            Id = Guid.Parse("77777777-0000-0000-0000-000000000004"),
            Name = "Fernanda Costa Lima",
            Email = "fernanda.lima@transportes.com",
            PasswordHash = PlaceholderPasswordHash,
            Role = UserRole.Motorista,
            Active = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Users.AddRange(
            colaborador1, colaborador2,
            clienteDistrib, clienteFarma, clienteMetalurgica, clienteSuper,
            motoristaCarlos, motoristaAna, motoristaRoberto, motoristaFernanda);

        // ── ClientProfiles ────────────────────────────────────────────────────
        context.ClientProfiles.AddRange(
            new ClientProfile
            {
                UserId = clienteDistrib.Id,
                Cnpj = "12.345.678/0001-90",
                Phone = "(11) 3001-2345",
                Address = "Rua das Indústrias, 500, Vila Industrial, São Paulo - SP"
            },
            new ClientProfile
            {
                UserId = clienteFarma.Id,
                Cnpj = "34.567.890/0001-12",
                Phone = "(11) 3003-4567",
                Address = "Rua Vergueiro, 3185, Saúde, São Paulo - SP"
            },
            new ClientProfile
            {
                UserId = clienteMetalurgica.Id,
                Cnpj = "23.456.789/0001-01",
                Phone = "(11) 3002-3456",
                Address = "Av. das Nações Unidas, 12901, Brooklin, São Paulo - SP"
            },
            new ClientProfile
            {
                UserId = clienteSuper.Id,
                Cnpj = "45.678.901/0001-23",
                Phone = "(11) 3004-5678",
                Address = "Av. Paulista, 100, Bela Vista, São Paulo - SP"
            });

        // ── DriverProfiles ────────────────────────────────────────────────────
        context.DriverProfiles.AddRange(
            new DriverProfile
            {
                UserId = motoristaCarlos.Id,
                Cnh = "12345678901",
                AdmissionDate = new DateOnly(2022, 3, 15),
                Active = true
            },
            new DriverProfile
            {
                UserId = motoristaAna.Id,
                Cnh = "98765432100",
                AdmissionDate = new DateOnly(2021, 7, 1),
                Active = true
            },
            new DriverProfile
            {
                UserId = motoristaRoberto.Id,
                Cnh = "55512345678",
                AdmissionDate = new DateOnly(2023, 1, 10),
                Active = true
            },
            new DriverProfile
            {
                UserId = motoristaFernanda.Id,
                Cnh = "33398765432",
                AdmissionDate = new DateOnly(2020, 11, 20),
                Active = true
            });

        // ── Vehicles ──────────────────────────────────────────────────────────
        var vehicleFiorino = new Vehicle
        {
            Id = Guid.Parse("33333333-0000-0000-0000-000000000001"),
            LicensePlate = "FUR1A23",
            Model = "Fiat Fiorino Furgão 1.3",
            CapacityKg = 500,
            CapacityM3 = 2.5m,
            ManufactureYear = 2021,
            Active = true,
            LastMaintenance = new DateOnly(2026, 4, 15),
            CreatedAt = now,
            UpdatedAt = now
        };

        var vehicleIveco = new Vehicle
        {
            Id = Guid.Parse("33333333-0000-0000-0000-000000000002"),
            LicensePlate = "IVE2B34",
            Model = "Iveco Daily 35S14",
            CapacityKg = 3500,
            CapacityM3 = 18,
            ManufactureYear = 2022,
            Active = true,
            LastMaintenance = new DateOnly(2026, 5, 10),
            CreatedAt = now,
            UpdatedAt = now
        };

        var vehicleSprinter = new Vehicle
        {
            Id = Guid.Parse("33333333-0000-0000-0000-000000000003"),
            LicensePlate = "MBS3C45",
            Model = "Mercedes-Benz Sprinter 415 CDI",
            CapacityKg = 1800,
            CapacityM3 = 12,
            ManufactureYear = 2023,
            Active = true,
            LastMaintenance = new DateOnly(2026, 5, 20),
            CreatedAt = now,
            UpdatedAt = now
        };

        var vehicleVW = new Vehicle
        {
            Id = Guid.Parse("33333333-0000-0000-0000-000000000004"),
            LicensePlate = "VWD4D56",
            Model = "Volkswagen Delivery 11.180",
            CapacityKg = 7500,
            CapacityM3 = 40,
            ManufactureYear = 2020,
            Active = true,
            LastMaintenance = new DateOnly(2026, 3, 30),
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Vehicles.AddRange(vehicleFiorino, vehicleIveco, vehicleSprinter, vehicleVW);

        // ── PickupRequests ────────────────────────────────────────────────────
        var req1 = new PickupRequest
        {
            Id = Guid.Parse("44444444-0000-0000-0000-000000000001"),
            IdentificationNumber = "COL-2026-0001",
            UserId = clienteDistrib.Id,
            Sender = "João Pereira",
            PickupAddress = "Rua das Indústrias, 500, Vila Industrial, São Paulo - SP",
            Recipient = "Maria Souza",
            DeliveryAddress = "Av. Eng. Luiz Carlos Berrini, 1376, Itaim Bibi, São Paulo - SP",
            RequestDate = new DateTime(2026, 6, 5, 9, 0, 0, DateTimeKind.Utc),
            ScheduledPickupDate = new DateOnly(2026, 6, 12),
            Priority = Priority.Normal,
            Status = PickupRequestStatus.Aberta,
            Notes = "Carga frágil. Manusear com cuidado.",
            CreatedAt = now,
            UpdatedAt = now
        };

        var req2 = new PickupRequest
        {
            Id = Guid.Parse("44444444-0000-0000-0000-000000000002"),
            IdentificationNumber = "COL-2026-0002",
            UserId = clienteMetalurgica.Id,
            Sender = "Pedro Alves",
            PickupAddress = "Av. das Nações Unidas, 12901, Brooklin, São Paulo - SP",
            Recipient = "Construtora São Jorge",
            DeliveryAddress = "Rua Funchal, 418, Vila Olímpia, São Paulo - SP",
            RequestDate = new DateTime(2026, 6, 6, 10, 30, 0, DateTimeKind.Utc),
            ScheduledPickupDate = new DateOnly(2026, 6, 11),
            Priority = Priority.Alta,
            Status = PickupRequestStatus.Atribuida,
            Notes = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        var req3 = new PickupRequest
        {
            Id = Guid.Parse("44444444-0000-0000-0000-000000000003"),
            IdentificationNumber = "COL-2026-0003",
            UserId = clienteFarma.Id,
            Sender = "Farmacêutica Vida Nova",
            PickupAddress = "Rua Vergueiro, 3185, Saúde, São Paulo - SP",
            Recipient = "Hospital das Clínicas",
            DeliveryAddress = "Av. Dr. Enéas de Carvalho Aguiar, 255, Pinheiros, São Paulo - SP",
            RequestDate = new DateTime(2026, 6, 8, 8, 0, 0, DateTimeKind.Utc),
            ScheduledPickupDate = new DateOnly(2026, 6, 11),
            Priority = Priority.Urgente,
            Status = PickupRequestStatus.EmAndamento,
            Notes = "Medicamentos controlados. Documentação obrigatória.",
            CreatedAt = now,
            UpdatedAt = now
        };

        var req4 = new PickupRequest
        {
            Id = Guid.Parse("44444444-0000-0000-0000-000000000004"),
            IdentificationNumber = "COL-2026-0004",
            UserId = clienteSuper.Id,
            Sender = "CD Supermercados Central",
            PickupAddress = "Av. Paulista, 100, Bela Vista, São Paulo - SP",
            Recipient = "Filial Mooca",
            DeliveryAddress = "Rua da Mooca, 1500, Mooca, São Paulo - SP",
            RequestDate = new DateTime(2026, 6, 1, 7, 0, 0, DateTimeKind.Utc),
            ScheduledPickupDate = new DateOnly(2026, 6, 2),
            Priority = Priority.Normal,
            Status = PickupRequestStatus.Concluida,
            Notes = "Entrega de reposição de estoque.",
            CreatedAt = now,
            UpdatedAt = now
        };

        var req5 = new PickupRequest
        {
            Id = Guid.Parse("44444444-0000-0000-0000-000000000005"),
            IdentificationNumber = "COL-2026-0005",
            UserId = clienteDistrib.Id,
            Sender = "Distribuidora Noroeste",
            PickupAddress = "Rua Errada, 999, Bairro Inexistente, São Paulo - SP",
            Recipient = "TechCorp Tecnologia",
            DeliveryAddress = "Av. Brigadeiro Faria Lima, 3500, Itaim Bibi, São Paulo - SP",
            RequestDate = new DateTime(2026, 6, 3, 14, 0, 0, DateTimeKind.Utc),
            ScheduledPickupDate = new DateOnly(2026, 6, 4),
            Priority = Priority.Baixa,
            Status = PickupRequestStatus.FalhaNaColeta,
            Notes = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        var req6 = new PickupRequest
        {
            Id = Guid.Parse("44444444-0000-0000-0000-000000000006"),
            IdentificationNumber = "COL-2026-0006",
            UserId = clienteMetalurgica.Id,
            Sender = "Metalúrgica BR",
            PickupAddress = "Av. das Nações Unidas, 12901, Brooklin, São Paulo - SP",
            Recipient = "Porto de Santos",
            DeliveryAddress = "Av. Conselheiro Rodrigues Alves, 17, Santos - SP",
            RequestDate = new DateTime(2026, 5, 28, 11, 0, 0, DateTimeKind.Utc),
            ScheduledPickupDate = new DateOnly(2026, 5, 30),
            Priority = Priority.Normal,
            Status = PickupRequestStatus.Cancelada,
            Notes = "Pedido cancelado pelo cliente antes da coleta.",
            CreatedAt = now,
            UpdatedAt = now
        };

        var req7 = new PickupRequest
        {
            Id = Guid.Parse("44444444-0000-0000-0000-000000000007"),
            IdentificationNumber = "COL-2026-0007",
            UserId = clienteFarma.Id,
            Sender = "Vida Nova Distribuidora",
            PickupAddress = "Rua Vergueiro, 3185, Saúde, São Paulo - SP",
            Recipient = "UBS Jabaquara",
            DeliveryAddress = "Rua dos Jequitibás, 1200, Jabaquara, São Paulo - SP",
            RequestDate = new DateTime(2026, 6, 10, 15, 0, 0, DateTimeKind.Utc),
            ScheduledPickupDate = new DateOnly(2026, 6, 13),
            Priority = Priority.Urgente,
            Status = PickupRequestStatus.Aberta,
            Notes = "Vacinas. Exige transporte refrigerado.",
            CreatedAt = now,
            UpdatedAt = now
        };

        var req8 = new PickupRequest
        {
            Id = Guid.Parse("44444444-0000-0000-0000-000000000008"),
            IdentificationNumber = "COL-2026-0008",
            UserId = clienteSuper.Id,
            Sender = "CD Supermercados Central",
            PickupAddress = "Av. Paulista, 100, Bela Vista, São Paulo - SP",
            Recipient = "Filial Santo André",
            DeliveryAddress = "Av. Industrial, 600, Santo André - SP",
            RequestDate = new DateTime(2026, 6, 9, 6, 0, 0, DateTimeKind.Utc),
            ScheduledPickupDate = new DateOnly(2026, 6, 11),
            Priority = Priority.Alta,
            Status = PickupRequestStatus.EmAndamento,
            Notes = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.PickupRequests.AddRange(req1, req2, req3, req4, req5, req6, req7, req8);

        // ── Assignments (DriverId → users.id com role=Motorista) ──────────────
        var assign2 = new Assignment
        {
            Id = Guid.Parse("55555555-0000-0000-0000-000000000002"),
            PickupRequestId = req2.Id,
            DriverId = motoristaAna.Id,
            VehicleId = vehicleFiorino.Id,
            AssignmentDate = new DateTime(2026, 6, 7, 8, 0, 0, DateTimeKind.Utc),
            ActualStartDate = null,
            ActualEndDate = null,
            DriverNotes = "Aguardando janela de coleta do cliente.",
            CreatedAt = now,
            UpdatedAt = now
        };

        var assign3 = new Assignment
        {
            Id = Guid.Parse("55555555-0000-0000-0000-000000000003"),
            PickupRequestId = req3.Id,
            DriverId = motoristaCarlos.Id,
            VehicleId = vehicleIveco.Id,
            AssignmentDate = new DateTime(2026, 6, 9, 7, 0, 0, DateTimeKind.Utc),
            ActualStartDate = new DateTime(2026, 6, 11, 8, 30, 0, DateTimeKind.Utc),
            ActualEndDate = null,
            DriverNotes = "Documentação conferida. Iniciando rota.",
            CreatedAt = now,
            UpdatedAt = now
        };

        var assign4 = new Assignment
        {
            Id = Guid.Parse("55555555-0000-0000-0000-000000000004"),
            PickupRequestId = req4.Id,
            DriverId = motoristaRoberto.Id,
            VehicleId = vehicleSprinter.Id,
            AssignmentDate = new DateTime(2026, 6, 1, 18, 0, 0, DateTimeKind.Utc),
            ActualStartDate = new DateTime(2026, 6, 2, 7, 15, 0, DateTimeKind.Utc),
            ActualEndDate = new DateTime(2026, 6, 2, 10, 45, 0, DateTimeKind.Utc),
            DriverNotes = "Entrega realizada sem intercorrências.",
            CreatedAt = now,
            UpdatedAt = now
        };

        var assign5 = new Assignment
        {
            Id = Guid.Parse("55555555-0000-0000-0000-000000000005"),
            PickupRequestId = req5.Id,
            DriverId = motoristaFernanda.Id,
            VehicleId = vehicleVW.Id,
            AssignmentDate = new DateTime(2026, 6, 3, 20, 0, 0, DateTimeKind.Utc),
            ActualStartDate = new DateTime(2026, 6, 4, 9, 0, 0, DateTimeKind.Utc),
            ActualEndDate = null,
            DriverNotes = "Endereço de coleta não localizado.",
            CreatedAt = now,
            UpdatedAt = now
        };

        var assign8 = new Assignment
        {
            Id = Guid.Parse("55555555-0000-0000-0000-000000000008"),
            PickupRequestId = req8.Id,
            DriverId = motoristaCarlos.Id,
            VehicleId = vehicleVW.Id,
            AssignmentDate = new DateTime(2026, 6, 9, 18, 0, 0, DateTimeKind.Utc),
            ActualStartDate = new DateTime(2026, 6, 11, 7, 0, 0, DateTimeKind.Utc),
            ActualEndDate = null,
            DriverNotes = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Assignments.AddRange(assign2, assign3, assign4, assign5, assign8);

        // ── Occurrences (RegisteredById → users.id + snapshot do nome) ────────
        var occ1 = new Occurrence
        {
            Id = Guid.Parse("66666666-0000-0000-0000-000000000001"),
            PickupRequestId = req5.Id,
            Type = OccurrenceType.EnderecoIncorreto,
            Description = "O endereço informado na solicitação não existe. GPS não reconhece a rua e moradores locais desconhecem o número.",
            OccurrenceDate = new DateTime(2026, 6, 4, 9, 30, 0, DateTimeKind.Utc),
            RegisteredById = motoristaFernanda.Id,
            RegisteredByNameSnapshot = "Fernanda Costa Lima",
            Resolved = false,
            ResolutionNotes = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        var occ2 = new Occurrence
        {
            Id = Guid.Parse("66666666-0000-0000-0000-000000000002"),
            PickupRequestId = req3.Id,
            Type = OccurrenceType.RemetenteIndisponivel,
            Description = "Responsável pela carga não estava presente no momento da coleta. Aguardou 20 minutos.",
            OccurrenceDate = new DateTime(2026, 6, 11, 8, 45, 0, DateTimeKind.Utc),
            RegisteredById = motoristaCarlos.Id,
            RegisteredByNameSnapshot = "Carlos Eduardo Oliveira",
            Resolved = true,
            ResolutionNotes = "Responsável retornou e liberou a carga após 30 minutos de espera.",
            CreatedAt = now,
            UpdatedAt = now
        };

        var occ3 = new Occurrence
        {
            Id = Guid.Parse("66666666-0000-0000-0000-000000000003"),
            PickupRequestId = req4.Id,
            Type = OccurrenceType.CargaDivergente,
            Description = "Quantidade de volumes entregues difere do manifesto (48 caixas vs 50 previstas).",
            OccurrenceDate = new DateTime(2026, 6, 2, 10, 30, 0, DateTimeKind.Utc),
            RegisteredById = motoristaRoberto.Id,
            RegisteredByNameSnapshot = "Roberto Ferreira Silva",
            Resolved = true,
            ResolutionNotes = "Cliente confirmou recebimento. Diferença já ajustada na nota fiscal.",
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Occurrences.AddRange(occ1, occ2, occ3);

        await context.SaveChangesAsync();
    }
}
