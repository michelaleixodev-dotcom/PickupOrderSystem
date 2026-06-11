using PickupOrderSystem.Application.DTOs;

namespace PickupOrderSystem.Application.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
}
