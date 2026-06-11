namespace PickupOrderSystem.Domain.Entities;

public class Driver
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Cnh { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public bool Active { get; set; } = true;
    public DateOnly AdmissionDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
