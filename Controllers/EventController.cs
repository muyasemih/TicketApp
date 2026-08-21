using Microsoft.AspNetCore.Mvc;
using TicketApp.Models;
using TicketApp.Repositories;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventRepository _repository;

    public EventsController(IEventRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetEvents()
    {
        var events = await _repository.GetAllAsync();

        return Ok(events);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEventById(int id)
    {
        var eventItem = await _repository.GetByIdAsync(id);

        if (eventItem == null)
        {
            return NotFound();
        }

        return Ok(eventItem);
    }
    [HttpPost]
    public async Task<IActionResult> CreateEvent(Event newEvent)
    {
        await _repository.AddAsync(newEvent);

        return Ok(newEvent);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(int id, Event updatedEvent)
    {
        var eventItem = await _repository.GetByIdAsync(id);

        if (eventItem == null)
        {
            return NotFound();
        }

        eventItem.Name = updatedEvent.Name;
        eventItem.Price = updatedEvent.Price;

        await _repository.UpdateAsync(eventItem);

        return Ok(eventItem);
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
         var eventItem = await _repository.GetByIdAsync(id);

        if (eventItem == null)
        {
            return NotFound();
        }

        await _repository.DeleteAsync(eventItem);
        return Ok(eventItem);

    }
}