using Framework.Endpoints;
using Microsoft.AspNetCore.Mvc;
using SeatReservation.Application.Reservations;
using SeatReservation.Contracts;

namespace SeatReservationService.Web.Controllers;

[ApiController]
[Route("api/reservations")]
public class ReservationsController : Controller
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Reserve(
        [FromBody]ReserveRequest request,
        [FromServices]ReserveHandler handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(request, cancellationToken);
    }

    [HttpPost("adjacent")]
    public async Task<EndpointResult<Guid>> ReserveAdjacentSeats(
        [FromBody] ReserveAdjacentSeatsRequest request,
        [FromServices] ReserveAdjacentSeatsHandler handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(request, cancellationToken);
    }
}
