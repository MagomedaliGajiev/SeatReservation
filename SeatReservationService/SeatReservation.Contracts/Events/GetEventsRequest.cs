namespace SeatReservation.Contracts.Events;

public record GetEventsRequest(
    string? Search,
    string? EventType,
    DateTime? DateFrom,
    DateTime? DateTo,
    string? Status,
    Guid? VenueId,
    int? MinAvailableSeats,
    PaginationRequest Pagination);

public record PaginationRequest(
    int Page = 1,
    int PageSize = 20);