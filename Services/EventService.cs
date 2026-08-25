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
            Price = e.Price,
            VenueId = e.VenueId,
            Venue = e.Venue == null ? null : new VenueDto
            {
                Id = e.Venue.Id,
                Name = e.Venue.Name,
                Blocks = e.Venue.Blocks.Select(b => new VenueBlockDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Capacity = b.Capacity
                }).ToList()
            }
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
            Price = eventItem.Price,
            VenueId = eventItem.VenueId,
            Venue = eventItem.Venue == null ? null : new VenueDto
            {
                Id = eventItem.Venue.Id,
                Name = eventItem.Venue.Name,
                Blocks = eventItem.Venue.Blocks.Select(b => new VenueBlockDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Capacity = b.Capacity
                }).ToList()
            }
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
        eventItem.VenueId = updatedEvent.VenueId;

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