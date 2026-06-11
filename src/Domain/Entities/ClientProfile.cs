namespace PickupOrderSystem.Domain.Entities;

public class ClientProfile
{
    public Guid UserId { get; set; }
    public string Cnpj { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Address { get; set; }

    public User User { get; set; } = null!;
}
