using Core.Validation;
using FluentValidation;
using SeatReservation.Contracts.Reservation;
using SharedKernel;

namespace SeatReservation.Application.Reservations.Commands;

public class ReserveRequestValidator : AbstractValidator<ReserveRequest>
{
    public ReserveRequestValidator()
    {
        RuleFor(r => r.EventId)
            .NotEmpty()
            .WithError(Error.Validation(
                "reservation.eventId", "Event ID cannot be empty", nameof(ReserveRequest.EventId)));

        RuleFor(r => r.UserId)
            .NotEmpty()
            .WithError(Error.Validation(
                "reservation.userId", "User ID cannot be empty", nameof(ReserveRequest.UserId)));

        RuleFor(r => r.SeatIds)
            .NotEmpty()
            .WithError(Error.Validation(
                "reservation.seats", "At least one seat must be selected", nameof(ReserveRequest.SeatIds)));

        RuleForEach(r => r.SeatIds)
            .NotEmpty()
            .WithError(Error.Validation(
                "reservation.seats", "Seat IDs cannot be empty", nameof(ReserveRequest.SeatIds)));
    }
}