using PickupOrderSystem.Domain.Entities;

namespace PickupOrderSystem.Application.Interfaces.Repositories;

public interface IVehicleRepository
{
    Task<List<Vehicle>> GetActiveVehiclesAsync();
    Task<Vehicle?> GetActiveByIdAsync(Guid id);
}
