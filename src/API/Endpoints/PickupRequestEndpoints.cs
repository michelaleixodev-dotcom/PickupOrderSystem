using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using PickupOrderSystem.Application.DTOs;
using PickupOrderSystem.Application.Interfaces;
using PickupOrderSystem.Domain.Exceptions;

namespace PickupOrderSystem.API.Endpoints;

public static class PickupRequestEndpoints
{
    public static IEndpointRouteBuilder MapPickupRequestEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/pickup-requests", async (ClaimsPrincipal user, IPickupRequestService service) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var role = user.FindFirstValue(ClaimTypes.Role)!;
            var items = await service.GetListAsync(role == "Cliente" ? userId : null);
            return Results.Ok(items);
        })
        .RequireAuthorization()
        .WithOpenApi();

        app.MapGet("/pickup-requests/{id:guid}", async (Guid id, ClaimsPrincipal user, IPickupRequestService service) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var role = user.FindFirstValue(ClaimTypes.Role)!;
            try
            {
                var dto = await service.GetByIdAsync(id, userId, role);
                return Results.Ok(dto);
            }
            catch (NotFoundException) { return Results.NotFound(); }
            catch (ForbiddenException) { return Results.Forbid(); }
        })
        .RequireAuthorization()
        .WithOpenApi();

        app.MapPost("/pickup-requests", async (CreatePickupRequestRequest body, ClaimsPrincipal user, IPickupRequestService service) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var userName = user.FindFirstValue(ClaimTypes.Name) ?? "Sistema";
            try
            {
                var (newId, identificationNumber) = await service.CreateAsync(body, userId, userName);
                return Results.Created($"/pickup-requests/{newId}", new { Id = newId, IdentificationNumber = identificationNumber });
            }
            catch (BusinessRuleException ex) { return Results.BadRequest(ex.Message); }
        })
        .RequireAuthorization()
        .WithOpenApi();

        app.MapPatch("/pickup-requests/{id:guid}/status", async (Guid id, UpdateStatusRequest body, ClaimsPrincipal user, IPickupRequestService service) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var userName = user.FindFirstValue(ClaimTypes.Name) ?? "Sistema";
            try
            {
                await service.UpdateStatusAsync(id, body.Status, userId, userName);
                return Results.NoContent();
            }
            catch (NotFoundException) { return Results.NotFound(); }
            catch (BusinessRuleException ex) { return Results.BadRequest(ex.Message); }
        })
        .RequireAuthorization("Colaborador")
        .WithOpenApi();

        app.MapPost("/pickup-requests/{id:guid}/assign", async (Guid id, AssignRequest body, ClaimsPrincipal user, IPickupRequestService service) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var userName = user.FindFirstValue(ClaimTypes.Name) ?? "Sistema";
            try
            {
                await service.AssignAsync(id, body, userId, userName);
                return Results.NoContent();
            }
            catch (NotFoundException) { return Results.NotFound(); }
            catch (BusinessRuleException ex) { return Results.BadRequest(ex.Message); }
        })
        .RequireAuthorization("Colaborador")
        .WithOpenApi();

        app.MapPost("/pickup-requests/{id:guid}/occurrences", async (Guid id, CreateOccurrenceRequest body, ClaimsPrincipal user, IPickupRequestService service) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var userName = user.FindFirstValue(ClaimTypes.Name) ?? "Sistema";
            try
            {
                await service.RegisterOccurrenceAsync(id, body, userId, userName);
                return Results.Created($"/pickup-requests/{id}/occurrences", null);
            }
            catch (NotFoundException) { return Results.NotFound(); }
            catch (BusinessRuleException ex) { return Results.BadRequest(ex.Message); }
        })
        .RequireAuthorization("Colaborador")
        .WithOpenApi();

        app.MapPost("/pickup-requests/{id:guid}/fail", async (Guid id, RegisterFailureRequest body, ClaimsPrincipal user, IPickupRequestService service) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var userName = user.FindFirstValue(ClaimTypes.Name) ?? "Sistema";
            try
            {
                await service.RegisterFailureAsync(id, body, userId, userName);
                return Results.NoContent();
            }
            catch (NotFoundException) { return Results.NotFound(); }
            catch (BusinessRuleException ex) { return Results.BadRequest(ex.Message); }
        })
        .RequireAuthorization("Colaborador")
        .WithOpenApi();

        return app;
    }
}
