using TicketApp.DTOs;
using TicketApp.Models;
using TicketApp.Repositories;

namespace TicketApp.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _repository;

    public EventService(IEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<EventDto>> GetAllAsync()
    {
        var events = await _repository.GetAllAsync();

        return events.Select(e => new EventDto
        {
            Id = e.Id,
            Name = e.Name,
            Price = e.Price
        }).ToList();
    }

    public async Task<EventDto?> GetByIdAsync(int id)
    {
        var eventItem = await _repository.GetByIdAsync(id);

        if (eventItem == null)
        {
            return null;
        }

        return new EventDto
        {
            Id = eventItem.Id,
            Name = eventItem.Name,
            Price = eventItem.Price
        };
    }

    public async Task<Event> CreateAsync(Event newEvent)
    {
        await _repository.AddAsync(newEvent);

        return newEvent;
    }

    public async Task<Event?> UpdateAsync(int id, Event updatedEvent)
    {
        var eventItem = await _repository.GetByIdAsync(id);

        if (eventItem == null)
        {
            return null;
        }

        eventItem.Name = updatedEvent.Name;
        eventItem.Price = updatedEvent.Price;

        await _repository.UpdateAsync(eventItem);

        return eventItem;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var eventItem = await _repository.GetByIdAsync(id);

        if (eventItem == null)
        {
            return false;
        }

        await _repository.DeleteAsync(eventItem);

        return true;
    }
}