namespace PickupOrderSystem.Application.DTOs;

public record LoginResponse(string Token, string Role, string Name, DateTime ExpiresAt);
