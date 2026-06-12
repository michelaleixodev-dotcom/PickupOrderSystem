using PickupOrderSystem.Domain.Entities;

namespace PickupOrderSystem.Application.Interfaces.Repositories;

public interface IPickupRequestRepository
{
    Task<List<PickupRequest>> GetAllAsync(Guid? userId = null);
    Task<PickupRequest?> GetByIdAsync(Guid id);
    Task<string?> GetLastIdentificationNumberAsync(string prefix);
    void Add(PickupRequest request);
    void AddStatusHistory(StatusHistory history);
    void AddAssignment(Assignment assignment);
    void AddOccurrence(Occurrence occurrence);
}
