namespace TicketApp.Services;

using TicketApp.Models;
using TicketApp.DTOs;

public interface IEventService
{
    Task<List<EventDto>> GetAllAsync();
    Task<EventDto?> GetByIdAsync(int id);
    Task<Event> CreateAsync(Event newEvent);
    Task<Event?> UpdateAsync(int id, Event updatedEvent);
    Task<bool> DeleteAsync(int id);

    Task<EventSeat?> ReserveSeatAsync(int eventId, int seatId);

    Task<EventSeat?> SellSeatAsync(int eventId, int seatId);
}