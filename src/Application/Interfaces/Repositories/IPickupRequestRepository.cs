using PickupOrderSystem.Domain.Entities;

namespace PickupOrderSystem.Application.Interfaces.Repositories;

public interface IPickupRequestRepository
{
    Task<List<PickupRequest>> GetAllAsync(Guid? userId = null, string? status = null, string? clientName = null, DateOnly? from = null, DateOnly? to = null, int page = 1, int pageSize = 10);
    Task<int> CountAsync(Guid? userId = null, string? status = null, string? clientName = null, DateOnly? from = null, DateOnly? to = null);
    Task<PickupRequest?> GetByIdAsync(Guid id);
    Task<string?> GetLastIdentificationNumberAsync(string prefix);
    void Add(PickupRequest request);
    void AddStatusHistory(StatusHistory history);
    void AddAssignment(Assignment assignment);
    void AddOccurrence(Occurrence occurrence);
}
