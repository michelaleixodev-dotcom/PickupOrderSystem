namespace PickupOrderSystem.Domain.Entities;

public class Client
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Cnpj { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Address { get; set; } = null!;
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
