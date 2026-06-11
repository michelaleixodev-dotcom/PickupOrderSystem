using PickupOrderSystem.Domain.Entities;

namespace PickupOrderSystem.Application.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) Generate(User user);
}
