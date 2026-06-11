using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PickupOrderSystem.Application.DTOs;
using PickupOrderSystem.Infrastructure.Data;

namespace PickupOrderSystem.Application.Services;

public class AuthService(AppDbContext db, ITokenService tokenService) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var passwordHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Password)));

        var user = await db.Users.FirstOrDefaultAsync(u =>
            u.Email == request.Email && u.PasswordHash == passwordHash && u.Active);

        if (user is null)
            return null;

        var (token, expiresAt) = tokenService.Generate(user);
        return new LoginResponse(token, user.Type.ToString(), user.Name, expiresAt);
    }
}
