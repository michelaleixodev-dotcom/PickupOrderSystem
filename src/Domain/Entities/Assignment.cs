namespace PickupOrderSystem.Domain.Entities;

public class Assignment
{
    public Guid Id { get; set; }
    public Guid PickupRequestId { get; set; }
    public Guid DriverId { get; set; }
    public Guid VehicleId { get; set; }
    public DateTime AssignmentDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public string? DriverNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public PickupRequest PickupRequest { get; set; } = null!;
    public Driver Driver { get; set; } = null!;
    public Vehicle Vehicle { get; set; } = null!;
}
