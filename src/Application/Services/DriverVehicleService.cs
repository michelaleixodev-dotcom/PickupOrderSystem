using PickupOrderSystem.Application.DTOs;
using PickupOrderSystem.Application.Interfaces;
using PickupOrderSystem.Application.Interfaces.Repositories;

namespace PickupOrderSystem.Application.Services;

public class DriverVehicleService(
    IDriverRepository driverRepo,
    IVehicleRepository vehicleRepo) : IDriverVehicleService
{
    public async Task<IReadOnlyList<DriverDto>> GetDriversAsync()
    {
        var drivers = await driverRepo.GetActiveDriversAsync();
        return drivers.Select(d => new DriverDto(d.Id, d.Name)).ToList();
    }

    public async Task<IReadOnlyList<VehicleDto>> GetVehiclesAsync()
    {
        var vehicles = await vehicleRepo.GetActiveVehiclesAsync();
        return vehicles.Select(v => new VehicleDto(v.Id, v.Model, v.LicensePlate)).ToList();
    }
}
