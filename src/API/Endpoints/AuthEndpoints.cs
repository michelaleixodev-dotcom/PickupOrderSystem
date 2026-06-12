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
        .WithTags("Autenticação")
        .WithSummary("Realiza login e retorna o token JWT")
        .WithDescription("""
            Autentica o usuário e retorna um token JWT para ser usado nas demais rotas.

            **Usuários de teste:**
            - Colaborador: `lucas.mendes@pickupsystem.com` / `Senha@123`
            - Cliente: `contato@distribnoroeste.com.br` / `Senha@123`
            """)
        .WithOpenApi()
        .AllowAnonymous();

        return app;
    }
}
