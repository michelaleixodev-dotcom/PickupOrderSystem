using Moq;
using PickupOrderSystem.Application.DTOs;
using PickupOrderSystem.Application.Interfaces;
using PickupOrderSystem.Application.Interfaces.Repositories;
using PickupOrderSystem.Application.Services;
using PickupOrderSystem.Domain.Entities;
using PickupOrderSystem.Domain.Enums;
using PickupOrderSystem.Domain.Exceptions;

namespace PickupOrderSystem.Tests.Services;

public class PickupRequestServiceTests
{
    private readonly Mock<IPickupRequestRepository> _repoMock = new();
    private readonly Mock<IDriverRepository> _driverMock = new();
    private readonly Mock<IVehicleRepository> _vehicleMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly PickupRequestService _service;

    public PickupRequestServiceTests()
    {
        _service = new PickupRequestService(
            _repoMock.Object,
            _driverMock.Object,
            _vehicleMock.Object,
            _uowMock.Object);
    }

    // ── Helpers ───────────────────────────────────────────────────
    private static PickupRequest BuildRequest(Guid ownerId, PickupRequestStatus status = PickupRequestStatus.Aberta) =>
        new()
        {
            Id = Guid.NewGuid(),
            IdentificationNumber = "COL-2026-0001",
            UserId = ownerId,
            Sender = "Remetente Teste",
            PickupAddress = "Rua A, 100",
            Recipient = "Destinatário Teste",
            DeliveryAddress = "Rua B, 200",
            RequestDate = DateTime.UtcNow,
            ScheduledPickupDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            Priority = Priority.Normal,
            Status = status,
            User = new User { Id = ownerId, Name = "Cliente Teste", Email = "cliente@teste.com", Role = UserRole.Cliente },
            Assignments = [],
            StatusHistories = [],
            Occurrences = []
        };

    // ── Testes de autorização ─────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ClienteAcessandoProprioRequest_RetornaDados()
    {
        var clienteId = Guid.NewGuid();
        var request = BuildRequest(ownerId: clienteId);

        _repoMock.Setup(r => r.GetByIdAsync(request.Id)).ReturnsAsync(request);

        var result = await _service.GetByIdAsync(request.Id, requestingUserId: clienteId, role: "Cliente");

        Assert.Equal(request.IdentificationNumber, result.IdentificationNumber);
    }

    [Fact]
    public async Task GetByIdAsync_ClienteAcessandoRequestDeOutroCliente_LancaForbiddenException()
    {
        var dono = Guid.NewGuid();
        var outroCliente = Guid.NewGuid();
        var request = BuildRequest(ownerId: dono);

        _repoMock.Setup(r => r.GetByIdAsync(request.Id)).ReturnsAsync(request);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.GetByIdAsync(request.Id, requestingUserId: outroCliente, role: "Cliente"));
    }

    // ── Teste de transição de status inválida ─────────────────────

    [Fact]
    public async Task UpdateStatusAsync_TransicaoDeCanceladaParaEmColeta_LancaBusinessRuleException()
    {
        var request = BuildRequest(ownerId: Guid.NewGuid(), status: PickupRequestStatus.Cancelada);

        _repoMock.Setup(r => r.GetByIdAsync(request.Id)).ReturnsAsync(request);

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _service.UpdateStatusAsync(request.Id, "EmColeta", Guid.NewGuid(), "Colaborador Teste"));

        Assert.Contains("não é permitida", ex.Message);
    }
}
