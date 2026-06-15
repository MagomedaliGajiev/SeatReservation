using Framework.Endpoints;
using Microsoft.AspNetCore.Mvc;
using SeatReservation.Application.Venues;
using SeatReservation.Contracts;
using SeatReservation.Contracts.Venues;

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

    [HttpPatch("/name")]
    public async Task<EndpointResult<Guid>> UpdateVenueName(
        [FromServices] UpdateVenueNameHandler handler,
        [FromBody] UpdateVenueNameRequest request,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(request, cancellationToken);
    }

    [HttpPatch("/name/by-prefix")]
    public async Task<IActionResult> UpdateVenueNameByPrefix(
        [FromServices] UpdateVenueNameByPrefixHandler handler,
        [FromBody] UpdateVenueNameByPrefixRequest request,
        CancellationToken cancellationToken)
    {
        await handler.Handle(request, cancellationToken);

        return Ok();
    }

    [HttpPatch("/seats")]
    public async Task<IActionResult> UpdateVenueSeats(
        [FromServices] UpdateVenueSeatsHandler handler,
        [FromBody] UpdateVenueSeatsRequest request,
        CancellationToken cancellationToken)
    {
        await handler.Handle(request, cancellationToken);

        return Ok();
    }
}