namespace PickupOrderSystem.Domain.Entities;

public class DriverProfile
{
    public Guid UserId { get; set; }
    public string Cnh { get; set; } = null!;
    public DateOnly AdmissionDate { get; set; }
    public bool Active { get; set; } = true;

    public User User { get; set; } = null!;
}
