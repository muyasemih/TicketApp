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
            EventDate = e.EventDate,
            VenueId = e.VenueId,
            Venue = e.Venue == null ? null : new VenueDto
            {
                Id = e.Venue.Id,
                Name = e.Venue.Name,
                Blocks = e.Venue.Blocks.Select(b => new VenueBlockDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Type = b.Type,
                    RowCount = b.RowCount,
                    SeatsPerRow = b.SeatsPerRow,
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
            EventDate = eventItem.EventDate,
            VenueId = eventItem.VenueId,
            Venue = eventItem.Venue == null ? null : new VenueDto
            {
                Id = eventItem.Venue.Id,
                Name = eventItem.Venue.Name,
                Blocks = eventItem.Venue.Blocks.Select(b => new VenueBlockDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Type = b.Type,
                    RowCount = b.RowCount,
                    SeatsPerRow = b.SeatsPerRow,
                    Capacity = b.Capacity
                }).ToList()
            }
        };
    }

    public async Task<Event> CreateAsync(Event newEvent)
    {
        if (newEvent.VenueId.HasValue)
        {
            var venue = await _repository.GetVenueWithSeatsAsync(newEvent.VenueId.Value);

            if (venue == null)
            {
                throw new ArgumentException(
                    $"Venue bulunamadı. Id: {newEvent.VenueId.Value}");
            }

            newEvent.Venue = venue;
        }

        await _repository.AddAsync(newEvent);

        if (newEvent.Venue != null)
        {
            var eventSeats = newEvent.Venue.Blocks
                .SelectMany(block => block.Seats)
                .Select(seat => new EventSeat
                {
                    EventId = newEvent.Id,
                    SeatId = seat.Id,
                    Status = EventSeatStatus.Available
                })
                .ToList();

            if (eventSeats.Count > 0)
            {
                await _repository.AddEventSeatsAsync(eventSeats);
            }
        }

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
        eventItem.EventDate = updatedEvent.EventDate;
        eventItem.VenueId = updatedEvent.VenueId;

        await _repository.UpdateAsync(eventItem);

        return eventItem;
    }

    public async Task<EventSeat?> ReserveSeatAsync(int eventId, int seatId)
    {
        var eventSeat = await _repository.GetEventSeatAsync(eventId, seatId);

        if (eventSeat == null)
        {
            return null;
        }

        var now = DateTime.UtcNow;

        if (eventSeat.Status == EventSeatStatus.Reserved &&
            eventSeat.ReservedUntil.HasValue &&
            eventSeat.ReservedUntil.Value <= now)
        {
            eventSeat.Status = EventSeatStatus.Available;
            eventSeat.ReservedUntil = null;
        }

        if (eventSeat.Status != EventSeatStatus.Available)
        {
            return null;
        }

        eventSeat.Status = EventSeatStatus.Reserved;
        eventSeat.ReservedUntil = now.AddMinutes(10);

        await _repository.UpdateEventSeatAsync(eventSeat);

        return eventSeat;
    }

    public async Task<EventSeat?> SellSeatAsync(int eventId, int seatId)
    {
        var eventSeat = await _repository.GetEventSeatAsync(eventId, seatId);

        if (eventSeat == null)
        {
            return null;
        }

        if (eventSeat.Status != EventSeatStatus.Reserved)
        {
            return null;
        }

        if (!eventSeat.ReservedUntil.HasValue ||
            eventSeat.ReservedUntil.Value <= DateTime.UtcNow)
        {
            eventSeat.Status = EventSeatStatus.Available;
            eventSeat.ReservedUntil = null;

            await _repository.UpdateEventSeatAsync(eventSeat);

            return null;
        }

        eventSeat.Status = EventSeatStatus.Sold;
        eventSeat.ReservedUntil = null;

        await _repository.UpdateEventSeatAsync(eventSeat);

        return eventSeat;
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