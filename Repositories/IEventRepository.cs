using TicketApp.Models;

namespace TicketApp.Repositories;

public interface IEventRepository
{
    Task<List<Event>> GetAllAsync();

    Task<Event?> GetByIdAsync(int id);

    Task AddAsync(Event newEvent);

    Task AddEventSeatsAsync(List<EventSeat> eventSeats);

    Task UpdateAsync(Event eventItem);
    Task<Venue?> GetVenueWithSeatsAsync(int venueId);

    Task<EventSeat?> GetEventSeatAsync(int eventId, int seatId);

    Task UpdateEventSeatAsync(EventSeat eventSeat);
    Task DeleteAsync(Event eventItem);
}