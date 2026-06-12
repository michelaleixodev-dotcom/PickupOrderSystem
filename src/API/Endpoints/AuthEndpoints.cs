using PickupOrderSystem.Application.DTOs;
using PickupOrderSystem.Application.Interfaces;
using PickupOrderSystem.Application.Services;

namespace PickupOrderSystem.API.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", async (LoginRequest request, IAuthService authService) =>
        {
            var result = await authService.LoginAsync(request);
            return result is null ? Results.Unauthorized() : Results.Ok(result);
        })
        .WithName("Login")
        .WithOpenApi()
        .AllowAnonymous();

        return app;
    }
}
