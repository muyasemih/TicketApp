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
            Price = newEvent.Price
        };

        await _service.CreateAsync(eventItem);

        return Ok(eventItem);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(int id, UpdateEventDto updatedEvent)
    {
        var eventItem = new Event
        {
            Name = updatedEvent.Name,
            Price = updatedEvent.Price
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
        return Ok(eventItem);

    }
}