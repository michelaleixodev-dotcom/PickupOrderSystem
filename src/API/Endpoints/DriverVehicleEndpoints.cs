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
        .WithOpenApi();

        app.MapGet("/vehicles", async (IDriverVehicleService service) =>
        {
            var vehicles = await service.GetVehiclesAsync();
            return Results.Ok(vehicles);
        })
        .RequireAuthorization("Colaborador")
        .WithOpenApi();

        return app;
    }
}
