namespace PickupOrderSystem.Domain.Entities;

public class StatusHistory
{
    public Guid Id { get; set; }
    public Guid PickupRequestId { get; set; }
    public string? FromStatus { get; set; }
    public string ToStatus { get; set; } = null!;
    public DateTime ChangedAt { get; set; }
    public Guid ChangedById { get; set; }
    public string ChangedByNameSnapshot { get; set; } = null!;

    public PickupRequest PickupRequest { get; set; } = null!;
}
