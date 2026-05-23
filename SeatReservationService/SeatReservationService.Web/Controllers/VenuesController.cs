using Framework.Endpoints;
using Microsoft.AspNetCore.Mvc;
using SeatReservation.Application;
using SeatReservation.Contracts;

namespace SeatReservationService.Web.Controllers;

[ApiController]
[Route("api/venues")]
public class VenuesController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] CreateVenueHandler handler,
        [FromBody] CreateVenueRequest request,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(request, cancellationToken);
    }
}