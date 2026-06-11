namespace PickupOrderSystem.Domain.Entities;

public class Vehicle
{
    public Guid Id { get; set; }
    public string LicensePlate { get; set; } = null!;
    public string Model { get; set; } = null!;
    public decimal CapacityKg { get; set; }
    public decimal CapacityM3 { get; set; }
    public int ManufactureYear { get; set; }
    public bool Active { get; set; } = true;
    public DateOnly? LastMaintenance { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
