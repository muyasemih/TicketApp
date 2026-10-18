namespace TicketApp.Services;

using TicketApp.Models;
using TicketApp.DTOs;

public interface IEventService
{
    Task<List<EventDto>> GetAllAsync(string? search = null, int? venueId = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 50);
    Task<EventDto?> GetByIdAsync(int id);
    Task<List<EventSeat>> GetSeatsByEventIdAsync(int eventId);
    Task<Event> CreateAsync(Event newEvent);
    Task<Event?> UpdateAsync(int id, Event updatedEvent);
    Task<bool> DeleteAsync(int id);
    Task<EventSeat?> ReserveSeatAsync(int eventId, int seatId);
    Task<EventSeat?> SellSeatAsync(int eventId, int seatId);
}
