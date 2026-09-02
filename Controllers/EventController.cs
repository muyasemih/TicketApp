using Microsoft.AspNetCore.Mvc;
using TicketApp.Models;
using TicketApp.Services;
using TicketApp.DTOs;
using Microsoft.AspNetCore.Authorization;

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
    [HttpGet("{eventId}/seats")]
    public async Task<IActionResult> GetEventSeats(int eventId)
    {
        var seats = await _service.GetEventSeatsAsync(eventId);

        var result = seats.Select(seat => new
        {
            id = seat.Id,
            eventId = seat.EventId,
            seatId = seat.SeatId,
            status = seat.Status,
            reservedUntil = seat.ReservedUntil,

            venueBlockId = seat.Seat?.VenueBlockId
        });

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateEvent(CreateEventDto newEvent)
    {
        var eventItem = new Event
        {
            Name = newEvent.Name,
            EventDate = newEvent.EventDate!.Value,
            VenueId = newEvent.VenueId
        };

        foreach (var block in newEvent.Blocks)
        {
            eventItem.BlockPrices.Add(new EventBlockPrice
            {
                VenueBlockId = block.VenueBlockId,
                Price = block.Price
            });
        }

        await _service.CreateAsync(eventItem);

        return CreatedAtAction(
            nameof(GetEventById),
            new { id = eventItem.Id },
            eventItem
        );
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(
        int id,
        UpdateEventDto updatedEvent)
    {
        var eventItem = new Event
        {
            Name = updatedEvent.Name,
            EventDate = updatedEvent.EventDate!.Value,
            VenueId = updatedEvent.VenueId
        };

        foreach (var block in updatedEvent.Blocks)
        {
            eventItem.BlockPrices.Add(new EventBlockPrice
            {
                VenueBlockId = block.VenueBlockId,
                Price = block.Price
            });
        }

        var result = await _service.UpdateAsync(id, eventItem);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
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

    [Authorize]
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