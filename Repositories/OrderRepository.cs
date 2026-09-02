using Microsoft.EntityFrameworkCore;
using TicketApp.Models;

namespace TicketApp.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;

    public OrderRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Order> CreateAsync(Order order)
    {
        _db.Orders.Add(order);

        await _db.SaveChangesAsync();

        return order;
    }

    public async Task<List<Order>> GetByUserIdAsync(int userId)
    {
        return await _db.Orders
            .Include(o => o.Items)
            .ThenInclude(oi => oi.EventSeat)
            .Include(o => o.Items)
            .ThenInclude(oi => oi.Ticket)
            .Where(o => o.UserId == userId)
            .ToListAsync();
    }

    public async Task<List<EventSeat>> GetEventSeatsAsync(
        int eventId,
        List<int> eventSeatIds)
    {
        return await _db.EventSeats
            .Include(es => es.Seat)
            .ThenInclude(s => s.VenueBlock)
            .Where(es =>
                es.EventId == eventId &&
                eventSeatIds.Contains(es.Id))
            .ToListAsync();
    }
}