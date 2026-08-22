namespace TicketApp.Services;
using TicketApp.Models;

public interface IEventService
{
    Task<List<Event>> GetAllAsync();
Task<Event?> GetByIdAsync(int id);
Task<Event> CreateAsync(Event newEvent);
Task<Event?> UpdateAsync(int id, Event updatedEvent);
Task<bool> DeleteAsync(int id);
}