using Framework.Endpoints;
using Microsoft.AspNetCore.Mvc;
using SeatReservation.Application.Events;
using SeatReservation.Contracts;

namespace SeatReservationService.Web.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] CreateEventHandler handler,
        [FromBody] CreateEventRequest request,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(request, cancellationToken);
    }
}