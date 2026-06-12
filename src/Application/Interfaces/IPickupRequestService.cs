using PickupOrderSystem.Application.DTOs;

namespace PickupOrderSystem.Application.Interfaces;

public interface IPickupRequestService
{
    Task<PagedResult<PickupRequestDto>> GetListAsync(Guid? userId, string? status = null, string? clientName = null, DateOnly? from = null, DateOnly? to = null, int page = 1, int pageSize = 10);
    Task<PickupRequestDto> GetByIdAsync(Guid id, Guid requestingUserId, string role);
    Task<(Guid Id, string IdentificationNumber)> CreateAsync(CreatePickupRequestRequest body, Guid userId, string userName);
    Task UpdateStatusAsync(Guid id, string status, Guid userId, string userName);
    Task AssignAsync(Guid id, AssignRequest body, Guid userId, string userName);
    Task RegisterOccurrenceAsync(Guid id, CreateOccurrenceRequest body, Guid userId, string userName);
    Task RegisterFailureAsync(Guid id, RegisterFailureRequest body, Guid userId, string userName);
}
