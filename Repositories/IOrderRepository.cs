using TicketApp.Models;

namespace TicketApp.Repositories;

public interface IOrderRepository
{
    Task<Order> CreateAsync(Order order);

    Task<List<Order>> GetByUserIdAsync(int userId);
    Task<List<EventSeat>> GetEventSeatsAsync(
    int eventId,
    List<int> eventSeatIds);
}