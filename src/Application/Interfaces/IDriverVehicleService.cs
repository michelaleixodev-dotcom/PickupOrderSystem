using PickupOrderSystem.Application.DTOs;

namespace PickupOrderSystem.Application.Interfaces;

public interface IDriverVehicleService
{
    Task<IReadOnlyList<DriverDto>> GetDriversAsync();
    Task<IReadOnlyList<VehicleDto>> GetVehiclesAsync();
}
