using PickupOrderSystem.Domain.Enums;

namespace PickupOrderSystem.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public UserType Type { get; set; }
    public bool Active { get; set; } = true;

    // Preenchidos apenas quando Type == Cliente
    public string? Cnpj { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
