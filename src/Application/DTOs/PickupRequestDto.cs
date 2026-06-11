namespace PickupOrderSystem.Application.DTOs;

public record PickupRequestDto(
    Guid Id,
    string IdentificationNumber,
    string ClientName,
    string Sender,
    string PickupAddress,
    string Recipient,
    string DeliveryAddress,
    DateTime RequestDate,
    DateOnly ScheduledPickupDate,
    string Priority,
    string Status,
    string? Notes
);

public record CreatePickupRequestRequest(
    string Sender,
    string PickupAddress,
    string Recipient,
    string DeliveryAddress,
    DateOnly ScheduledPickupDate,
    string Priority,
    string? Notes
);

public record UpdateStatusRequest(string Status);
