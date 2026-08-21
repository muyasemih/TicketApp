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
        return await _db.Events.ToListAsync();
    }
    public async Task<Event?> GetByIdAsync(int id)
    {
        return await _db.Events
            .FirstOrDefaultAsync(e=> e.Id == id);
    }
    public async Task AddAsync(Event newEvent)
    {
        _db.Events.Add(newEvent);
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