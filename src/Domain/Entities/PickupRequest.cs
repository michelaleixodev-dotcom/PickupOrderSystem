using PickupOrderSystem.Domain.Enums;

namespace PickupOrderSystem.Domain.Entities;

public class PickupRequest
{
    public Guid Id { get; set; }
    public string IdentificationNumber { get; set; } = null!;
    public Guid UserId { get; set; }
    public string Sender { get; set; } = null!;
    public string PickupAddress { get; set; } = null!;
    public string Recipient { get; set; } = null!;
    public string DeliveryAddress { get; set; } = null!;
    public DateTime RequestDate { get; set; }
    public DateOnly ScheduledPickupDate { get; set; }
    public Priority Priority { get; set; }
    public PickupRequestStatus Status { get; set; } = PickupRequestStatus.Aberta;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<Assignment> Assignments { get; set; } = [];
    public ICollection<Occurrence> Occurrences { get; set; } = [];
}
