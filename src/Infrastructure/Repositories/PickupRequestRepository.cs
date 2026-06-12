using Microsoft.EntityFrameworkCore;
using PickupOrderSystem.Application.Interfaces.Repositories;
using PickupOrderSystem.Domain.Entities;
using PickupOrderSystem.Infrastructure.Data;

namespace PickupOrderSystem.Infrastructure.Repositories;

public class PickupRequestRepository(AppDbContext db) : IPickupRequestRepository
{
    public Task<List<PickupRequest>> GetAllAsync(Guid? userId = null)
    {
        var query = db.PickupRequests.Include(r => r.User).AsQueryable();

        if (userId.HasValue)
            query = query.Where(r => r.UserId == userId.Value);

        return query.OrderByDescending(r => r.RequestDate).ToListAsync();
    }

    public Task<PickupRequest?> GetByIdAsync(Guid id) =>
        db.PickupRequests
            .Include(r => r.User)
            .Include(r => r.Assignments).ThenInclude(a => a.Driver)
            .Include(r => r.Assignments).ThenInclude(a => a.Vehicle)
            .Include(r => r.StatusHistories)
            .Include(r => r.Occurrences)
            .FirstOrDefaultAsync(r => r.Id == id);

    public Task<string?> GetLastIdentificationNumberAsync(string prefix) =>
        db.PickupRequests
            .Where(r => r.IdentificationNumber.StartsWith(prefix))
            .OrderByDescending(r => r.IdentificationNumber)
            .Select(r => r.IdentificationNumber)
            .FirstOrDefaultAsync();

    public void Add(PickupRequest request) => db.PickupRequests.Add(request);

    public void AddStatusHistory(StatusHistory history) => db.StatusHistories.Add(history);

    public void AddAssignment(Assignment assignment) => db.Assignments.Add(assignment);

    public void AddOccurrence(Occurrence occurrence) => db.Occurrences.Add(occurrence);
}
