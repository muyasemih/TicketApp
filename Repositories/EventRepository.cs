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

    public async Task<List<Event>> GetAllAsync(
        string? search = null, 
        int? venueId = null, 
        DateTime? startDate = null, 
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 50)
    {
        var query = _db.Events
            .AsNoTracking()
            .Include(e => e.Venue)
                .ThenInclude(v => v!.Blocks)
            .Include(e => e.BlockPrices)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(e => 
                e.Name.ToLower().Contains(term) || 
                (e.Venue != null && e.Venue.Name.ToLower().Contains(term)));
        }

        if (venueId.HasValue)
        {
            query = query.Where(e => e.VenueId == venueId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(e => e.EventDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(e => e.EventDate <= endDate.Value);
        }

        query = query.OrderBy(e => e.EventDate);

        if (page > 0 && pageSize > 0)
        {
            query = query.Skip((page - 1) * pageSize).Take(pageSize);
        }

        return await query.ToListAsync();
    }

    public async Task<Event?> GetByIdAsync(int id)
    {
        return await _db.Events
            .AsNoTracking()
            .Include(e => e.Venue)
                .ThenInclude(v => v!.Blocks)
            .Include(e => e.BlockPrices)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Venue?> GetVenueWithSeatsAsync(int venueId)
    {
        return await _db.Venues
            .Include(v => v.Blocks)
                .ThenInclude(b => b.Seats)
            .FirstOrDefaultAsync(v => v.Id == venueId);
    }

    public async Task<List<EventSeat>> GetEventSeatsAsync(int eventId)
    {
        return await _db.EventSeats
            .Include(es => es.Seat)
                .ThenInclude(s => s.VenueBlock)
            .Where(es => es.EventId == eventId)
            .ToListAsync();
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

    public async Task UpdateEventBlockPricesAsync(
        int eventId,
        List<EventBlockPrice> blockPrices)
    {
        var existingBlockPrices = await _db.EventBlockPrices
            .Where(bp => bp.EventId == eventId)
            .ToListAsync();

        _db.EventBlockPrices.RemoveRange(existingBlockPrices);

        foreach (var blockPrice in blockPrices)
        {
            blockPrice.EventId = eventId;
        }

        _db.EventBlockPrices.AddRange(blockPrices);

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

    public async Task ReplaceEventSeatsAsync(int eventId, List<EventSeat> newSeats)
    {
        var existing = await _db.EventSeats.Where(es => es.EventId == eventId).ToListAsync();
        _db.EventSeats.RemoveRange(existing);
        _db.EventSeats.AddRange(newSeats);
        await _db.SaveChangesAsync();
    }
}
