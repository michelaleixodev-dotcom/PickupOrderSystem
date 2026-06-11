namespace PickupOrderSystem.Application.DTOs;

public record LoginResponse(string Token, string Type, string Name, DateTime ExpiresAt);
