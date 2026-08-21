using TicketApp.Models;

namespace TicketApp.Repositories;

public interface IEventRepository
{
    Task<List<Event>> GetAllAsync();
    Task<Event?> GetByIdAsync(int id);
    Task AddAsync(Event newEvent);
    Task UpdateAsync(Event eventItem);
    Task DeleteAsync(Event eventItem);
}