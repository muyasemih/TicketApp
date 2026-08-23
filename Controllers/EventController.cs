using Microsoft.AspNetCore.Mvc;
using TicketApp.Models;
using TicketApp.Services;

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
    public async Task<IActionResult> CreateEvent(Event newEvent)
    {
        try
        {
            var eventItem = await _service.CreateAsync(newEvent);

            return Ok(eventItem);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                error = ex.Message
            });
        }
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(int id, Event updatedEvent)
    {
        var eventItem = await _service.GetByIdAsync(id);

        if (eventItem == null)
        {
            return NotFound();
        }

        eventItem.Name = updatedEvent.Name;
        eventItem.Price = updatedEvent.Price;

        await _service.UpdateAsync(id, updatedEvent);

        return Ok(eventItem);
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