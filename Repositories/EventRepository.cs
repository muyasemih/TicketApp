using Microsoft.EntityFrameworkCore;
using TicketApp.Models;

namespace TicketApp.Repositories;

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _db;

    public EventRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Event>> GetAllAsync()
    {
        return await _db.Events
            .Include(e => e.Venue)
            .ThenInclude(v => v.Blocks)
            .ThenInclude(b => b.Seats)
            .ToListAsync();
    }

    public async Task<Event?> GetByIdAsync(int id)
    {
        return await _db.Events
            .Include(e => e.Venue)
            .ThenInclude(v => v.Blocks)
            .ThenInclude(b => b.Seats)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Venue?> GetVenueWithSeatsAsync(int venueId)
    {
        return await _db.Venues
            .Include(v => v.Blocks)
            .ThenInclude(b => b.Seats)
            .FirstOrDefaultAsync(v => v.Id == venueId);
    }

    public async Task AddAsync(Event newEvent)
    {
        _db.Events.Add(newEvent);
        await _db.SaveChangesAsync();
    }

    public async Task AddEventSeatsAsync(List<EventSeat> eventSeats)
    {
        _db.EventSeats.AddRange(eventSeats);
        await _db.SaveChangesAsync();
    }

    public async Task<EventSeat?> GetEventSeatAsync(int eventId, int seatId)
{
    return await _db.EventSeats
        .FirstOrDefaultAsync(es =>
            es.EventId == eventId &&
            es.SeatId == seatId);
}

    public async Task UpdateEventSeatAsync(EventSeat eventSeat)
    {
        _db.EventSeats.Update(eventSeat);

        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Event eventItem)
    {
        _db.Events.Update(eventItem);

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Event eventItem)
    {
        _db.Events.Remove(eventItem);
        await _db.SaveChangesAsync();
    }
}