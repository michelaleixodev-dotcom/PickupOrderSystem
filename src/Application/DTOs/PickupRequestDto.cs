namespace PickupOrderSystem.Application.DTOs;

public record AssignmentDto(
    Guid Id,
    string DriverName,
    string VehiclePlate,
    string VehicleModel,
    DateTime AssignmentDate
);

public record StatusHistoryDto(
    string? FromStatus,
    string ToStatus,
    DateTime ChangedAt,
    string ChangedBy
);

public record OccurrenceDto(
    Guid Id,
    string Type,
    string Description,
    DateTime OccurrenceDate,
    string RegisteredBy,
    bool Resolved,
    string? ResolutionNotes
);

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
    string? Notes,
    AssignmentDto? Assignment,
    IReadOnlyList<StatusHistoryDto>? StatusHistory,
    IReadOnlyList<OccurrenceDto>? Occurrences
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

public record AssignRequest(Guid DriverId, Guid VehicleId);

public record CreateOccurrenceRequest(string Type, string Description);

public record RegisterFailureRequest(string Type, string Description);
