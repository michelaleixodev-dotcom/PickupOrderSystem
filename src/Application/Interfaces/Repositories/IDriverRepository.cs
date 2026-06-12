using PickupOrderSystem.Domain.Entities;

namespace PickupOrderSystem.Application.Interfaces.Repositories;

public interface IDriverRepository
{
    Task<List<User>> GetActiveDriversAsync();
    Task<User?> GetActiveByIdAsync(Guid id);
}
