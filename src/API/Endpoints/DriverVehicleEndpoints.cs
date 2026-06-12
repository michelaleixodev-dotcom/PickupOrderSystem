using PickupOrderSystem.Application.Interfaces;

namespace PickupOrderSystem.API.Endpoints;

public static class DriverVehicleEndpoints
{
    public static IEndpointRouteBuilder MapDriverVehicleEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/drivers", async (IDriverVehicleService service) =>
        {
            var drivers = await service.GetDriversAsync();
            return Results.Ok(drivers);
        })
        .RequireAuthorization("Colaborador")
        .WithTags("Motoristas e Veículos")
        .WithSummary("Lista motoristas ativos")
        .WithDescription("Retorna todos os motoristas com status ativo disponíveis para atribuição. Restrito a Colaboradores.")
        .WithOpenApi();

        app.MapGet("/vehicles", async (IDriverVehicleService service) =>
        {
            var vehicles = await service.GetVehiclesAsync();
            return Results.Ok(vehicles);
        })
        .RequireAuthorization("Colaborador")
        .WithTags("Motoristas e Veículos")
        .WithSummary("Lista veículos ativos")
        .WithDescription("Retorna todos os veículos com status ativo disponíveis para atribuição. Restrito a Colaboradores.")
        .WithOpenApi();

        return app;
    }
}
