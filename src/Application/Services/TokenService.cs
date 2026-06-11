using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PickupOrderSystem.Domain.Entities;

namespace PickupOrderSystem.Application.Services;

public class TokenService(IConfiguration config) : ITokenService
{
    public (string Token, DateTime ExpiresAt) Generate(User user)
    {
        var secret = config["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret não está configurado. Defina a variável de ambiente Jwt__Secret.");

        var minutes = int.Parse(config["Jwt:ExpirationMinutes"] ?? "60");
        var expiresAt = DateTime.UtcNow.AddMinutes(minutes);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
