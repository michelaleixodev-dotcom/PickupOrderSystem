using Microsoft.EntityFrameworkCore;
using PickupOrderSystem.Application.Interfaces.Repositories;
using PickupOrderSystem.Domain.Entities;
using PickupOrderSystem.Domain.Enums;
using PickupOrderSystem.Infrastructure.Data;

namespace PickupOrderSystem.Infrastructure.Repositories;

public class PickupRequestRepository(AppDbContext db) : IPickupRequestRepository
{
    public Task<List<PickupRequest>> GetAllAsync(Guid? userId = null, string? status = null, string? clientName = null, DateOnly? from = null, DateOnly? to = null, int page = 1, int pageSize = 10) =>
        BuildQuery(userId, status, clientName, from, to)
            .OrderByDescending(r => r.RequestDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public Task<int> CountAsync(Guid? userId = null, string? status = null, string? clientName = null, DateOnly? from = null, DateOnly? to = null) =>
        BuildQuery(userId, status, clientName, from, to).CountAsync();

    private IQueryable<PickupRequest> BuildQuery(Guid? userId, string? status, string? clientName, DateOnly? from, DateOnly? to)
    {
        var query = db.PickupRequests.Include(r => r.User).AsQueryable();

        if (userId.HasValue)
            query = query.Where(r => r.UserId == userId.Value);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PickupRequestStatus>(status, out var parsedStatus))
            query = query.Where(r => r.Status == parsedStatus);

        if (!string.IsNullOrWhiteSpace(clientName))
            query = query.Where(r => r.User.Name.Contains(clientName));

        if (from.HasValue)
            query = query.Where(r => r.ScheduledPickupDate >= from.Value);

        if (to.HasValue)
            query = query.Where(r => r.ScheduledPickupDate <= to.Value);

        return query;
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
