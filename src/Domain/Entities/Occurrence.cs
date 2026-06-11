using PickupOrderSystem.Domain.Enums;

namespace PickupOrderSystem.Domain.Entities;

public class Occurrence
{
    public Guid Id { get; set; }
    public Guid PickupRequestId { get; set; }
    public OccurrenceType Type { get; set; }
    public string Description { get; set; } = null!;
    public DateTime OccurrenceDate { get; set; }
    public string RegisteredBy { get; set; } = null!;
    public bool Resolved { get; set; } = false;
    public string? ResolutionNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public PickupRequest PickupRequest { get; set; } = null!;
}
