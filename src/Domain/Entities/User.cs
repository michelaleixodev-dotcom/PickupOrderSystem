using PickupOrderSystem.Domain.Enums;

namespace PickupOrderSystem.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public UserRole Role { get; set; }
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ClientProfile? ClientProfile { get; set; }
    public DriverProfile? DriverProfile { get; set; }
}
