using Microsoft.EntityFrameworkCore;
using PickupOrderSystem.Application.Interfaces.Repositories;
using PickupOrderSystem.Domain.Entities;
using PickupOrderSystem.Infrastructure.Data;

namespace PickupOrderSystem.Infrastructure.Repositories;

public class VehicleRepository(AppDbContext db) : IVehicleRepository
{
    public Task<List<Vehicle>> GetActiveVehiclesAsync() =>
        db.Vehicles
            .Where(v => v.Active)
            .OrderBy(v => v.Model)
            .ToListAsync();

    public Task<Vehicle?> GetActiveByIdAsync(Guid id) =>
        db.Vehicles.FirstOrDefaultAsync(v => v.Id == id && v.Active);
}
