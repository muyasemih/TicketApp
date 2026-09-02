using TicketApp.DTOs;
using TicketApp.Models;
using TicketApp.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TicketApp.Services;

public class EventService : IEventService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEventRepository _repository;

    public EventService(
        IEventRepository repository,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<EventDto>> GetAllAsync()
    {
        var events = await _repository.GetAllAsync();

        return events.Select(e => new EventDto
        {
            Id = e.Id,
            Name = e.Name,
            EventDate = e.EventDate,
            VenueId = e.VenueId,

            BlockPrices = e.BlockPrices.Select(bp => new EventBlockPriceDto
            {
                Id = bp.Id,
                VenueBlockId = bp.VenueBlockId,
                Price = bp.Price
            }).ToList(),

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
            EventDate = eventItem.EventDate,
            VenueId = eventItem.VenueId,

            BlockPrices = eventItem.BlockPrices.Select(bp => new EventBlockPriceDto
            {
                Id = bp.Id,
                VenueBlockId = bp.VenueBlockId,
                Price = bp.Price
            }).ToList(),

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
     public async Task<List<EventSeat>> GetEventSeatsAsync(int eventId)
        {
            var seats = await _repository.GetEventSeatsAsync(eventId);

            var now = DateTime.UtcNow;

            foreach (var seat in seats)
            {
                if (seat.Status == EventSeatStatus.Reserved &&
                    seat.ReservedUntil.HasValue &&
                    seat.ReservedUntil.Value <= now)
                {
                    seat.Status = EventSeatStatus.Available;
                    seat.ReservedUntil = null;
                    seat.ReservedByUserId = null;
                }
            }

            return seats;
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

        if (newEvent.Venue != null && newEvent.BlockPrices.Count > 0)
        {
            var validBlockIds = newEvent.Venue.Blocks
                .Select(block => block.Id)
                .ToHashSet();

            foreach (var blockPrice in newEvent.BlockPrices)
            {
                if (!validBlockIds.Contains(blockPrice.VenueBlockId))
                {
                    throw new ArgumentException(
                        $"Venue'ya ait olmayan block kullanılamaz. Block Id: {blockPrice.VenueBlockId}");
                }
            }
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

        if (updatedEvent.VenueId.HasValue)
        {
            var venue = await _repository.GetVenueWithSeatsAsync(updatedEvent.VenueId.Value);

            if (venue == null)
            {
                throw new ArgumentException(
                    $"Venue bulunamadı. Id: {updatedEvent.VenueId.Value}");
            }

            var validBlockIds = venue.Blocks
                .Select(block => block.Id)
                .ToHashSet();

            foreach (var blockPrice in updatedEvent.BlockPrices)
            {
                if (!validBlockIds.Contains(blockPrice.VenueBlockId))
                {
                    throw new ArgumentException(
                        $"Venue'ya ait olmayan block kullanılamaz. Block Id: {blockPrice.VenueBlockId}");
                }
            }
        }

        eventItem.Name = updatedEvent.Name;
        eventItem.EventDate = updatedEvent.EventDate;
        eventItem.VenueId = updatedEvent.VenueId;

        await _repository.UpdateAsync(eventItem);

        await _repository.UpdateEventBlockPricesAsync(
            id,
            updatedEvent.BlockPrices);

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
            eventSeat.ReservedByUserId = null;
        }

        if (eventSeat.Status != EventSeatStatus.Available)
        {
            return null;
        }

        var userIdClaim = _httpContextAccessor.HttpContext?
            .User
            .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?
            .Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        eventSeat.Status = EventSeatStatus.Reserved;
        eventSeat.ReservedUntil = now.AddMinutes(5);
        eventSeat.ReservedByUserId = userId;

        try
        {
            await _repository.UpdateEventSeatAsync(eventSeat);
        }
        catch (DbUpdateConcurrencyException)
        {
            return null;
        }

        return eventSeat;
    }

    public async Task<EventSeat?> SellSeatAsync(int eventId, int seatId)
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
            eventSeat.ReservedByUserId = null;

            try
            {
                await _repository.UpdateEventSeatAsync(eventSeat);
            }
            catch (DbUpdateConcurrencyException)
            {
                return null;
            }

            return null;
        }

        if (eventSeat.Status != EventSeatStatus.Reserved)
        {
            return null;
        }

        var userIdClaim = _httpContextAccessor.HttpContext?
            .User
            .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?
            .Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        if (eventSeat.ReservedByUserId != userId)
        {
            return null;
        }

        eventSeat.Status = EventSeatStatus.Sold;
        eventSeat.ReservedUntil = null;
        eventSeat.ReservedByUserId = null;

        try
        {
            await _repository.UpdateEventSeatAsync(eventSeat);
        }
        catch (DbUpdateConcurrencyException)
        {
            return null;
        }

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