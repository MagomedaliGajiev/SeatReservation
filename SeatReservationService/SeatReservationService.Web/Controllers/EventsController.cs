using Framework.Endpoints;
using Microsoft.AspNetCore.Mvc;
using SeatReservation.Application.Events.Commands;
using SeatReservation.Application.Events.Queries;
using SeatReservation.Contracts.Events;

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

    [HttpGet("{eventId:guid}")]
    public async Task<ActionResult<GetEventDto>> GetById(
        [FromRoute]Guid eventId,
        [FromServices] GetEventByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var @event = await handler.Handle(new GetEventByIdRequest(eventId), cancellationToken);
        return @event is null ? NotFound() : Ok(@event);
    }

    [HttpGet("{eventId:guid}/dapper")]
    public async Task<ActionResult<GetEventDto>> GetByIdDapper(
        [FromRoute]Guid eventId,
        [FromServices] GetEventByIdHandlerDapper handler,
        CancellationToken cancellationToken)
    {
        var @event = await handler.Handle(new GetEventByIdRequest(eventId), cancellationToken);
        return @event is null ? NotFound() : Ok(@event);
    }

    [HttpGet]
    public async Task<ActionResult<GetEventsDto>> Get(
        [FromQuery] GetEventsRequest request,
        [FromServices] GetEventsHandler handler,
        CancellationToken cancellationToken)
    {
       var events = await handler.Handle(request, cancellationToken);
       return Ok(events);
    }

    [HttpGet("dapper")]
    public async Task<ActionResult<GetEventsDto>> GetDapper(
        [FromQuery] GetEventsRequest request,
        [FromServices] GetEventsHandlerDapper handler,
        CancellationToken cancellationToken)
    {
        var events = await handler.Handle(request, cancellationToken);
        return Ok(events);
    }
}