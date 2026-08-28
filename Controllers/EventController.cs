using Microsoft.AspNetCore.Mvc;
using TicketApp.Models;
using TicketApp.Services;
using TicketApp.DTOs;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _service;

    public EventsController(IEventService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetEvents()
    {
        var events = await _service.GetAllAsync();

        return Ok(events);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEventById(int id)
    {
        var eventItem = await _service.GetByIdAsync(id);

        if (eventItem == null)
        {
            return NotFound();
        }

        return Ok(eventItem);
    }

    [HttpPost]
    public async Task<IActionResult> CreateEvent(CreateEventDto newEvent)
    {
        var eventItem = new Event
        {
            Name = newEvent.Name,
            Price = newEvent.Price,
            EventDate = newEvent.EventDate!.Value,
            VenueId = newEvent.VenueId
        };

        await _service.CreateAsync(eventItem);

        return CreatedAtAction(
            nameof(GetEventById),
            new { id = eventItem.Id },
            eventItem
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(
        int id,
        UpdateEventDto updatedEvent)
    {
        var eventItem = new Event
        {
            Name = updatedEvent.Name,
            Price = updatedEvent.Price,
            EventDate = updatedEvent.EventDate!.Value,
            VenueId = updatedEvent.VenueId
        };

        var result = await _service.UpdateAsync(id, eventItem);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var eventItem = await _service.GetByIdAsync(id);

        if (eventItem == null)
        {
            return NotFound();
        }

        await _service.DeleteAsync(id);

        return NoContent();
    }
    [HttpPost("{eventId}/seats/{seatId}/reserve")]
    public async Task<IActionResult> ReserveSeat(int eventId, int seatId)
    {
        var eventSeat = await _service.ReserveSeatAsync(eventId, seatId);

        if (eventSeat == null)
        {
            return BadRequest(new
            {
                error = "Koltuk mevcut değil veya şu anda rezerve edilemiyor."
            });
        }

        return Ok(eventSeat);
    }
    [HttpPost("{eventId}/seats/{seatId}/sell")]
    public async Task<IActionResult> SellSeat(int eventId, int seatId)
    {
        var eventSeat = await _service.SellSeatAsync(eventId, seatId);

        if (eventSeat == null)
        {
            return BadRequest(new
            {
                error = "Koltuk mevcut değil, rezerve edilmemiş veya rezervasyon süresi dolmuş."
            });
        }

        return Ok(eventSeat);
    }
}