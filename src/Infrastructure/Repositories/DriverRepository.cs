using Microsoft.EntityFrameworkCore;
using PickupOrderSystem.Application.Interfaces.Repositories;
using PickupOrderSystem.Domain.Entities;
using PickupOrderSystem.Domain.Enums;
using PickupOrderSystem.Infrastructure.Data;

namespace PickupOrderSystem.Infrastructure.Repositories;

public class DriverRepository(AppDbContext db) : IDriverRepository
{
    public Task<List<User>> GetActiveDriversAsync() =>
        db.Users
            .Where(u => u.Role == UserRole.Motorista && u.Active)
            .OrderBy(u => u.Name)
            .ToListAsync();

    public Task<User?> GetActiveByIdAsync(Guid id) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == UserRole.Motorista && u.Active);
}
