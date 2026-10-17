namespace TicketApp.Repositories;

using TicketApp.Models;

public interface IEventRepository
{
    Task<List<Event>> GetAllAsync(string? search = null, int? venueId = null, DateTime? startDate = null, DateTime? endDate = null);
    Task<Event?> GetByIdAsync(int id);
    Task<Venue?> GetVenueWithSeatsAsync(int venueId);
    Task<List<EventSeat>> GetEventSeatsAsync(int eventId);
    Task AddAsync(Event newEvent);
    Task AddEventSeatsAsync(List<EventSeat> eventSeats);
    Task<EventSeat?> GetEventSeatAsync(int eventId, int seatId);
    Task UpdateEventSeatAsync(EventSeat eventSeat);
    Task UpdateEventBlockPricesAsync(int eventId, List<EventBlockPrice> blockPrices);
    Task UpdateAsync(Event eventItem);
    Task DeleteAsync(Event eventItem);
}
