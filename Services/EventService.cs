using TicketApp.Models;
using TicketApp.Repositories;

namespace TicketApp.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _repository;

    public EventService(IEventRepository repository)
    {
        _repository = repository;
    }
    public async Task<List<Event>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }
    public async Task<Event?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
    public async Task<Event> CreateAsync(Event newEvent)
        {
            await _repository.AddAsync(newEvent);

            return newEvent;
        }
            public async Task<Event?> UpdateAsync(int id, Event updatedEvent)
            {
                var eventItem = await _repository.GetByIdAsync(id);

                if (eventItem == null)
                {
                    return null;
                }

                eventItem.Name = updatedEvent.Name;
                eventItem.Price = updatedEvent.Price;

                await _repository.UpdateAsync(eventItem);

                return eventItem;
            }
            public async Task<bool> DeleteAsync(int id)
            {
                var eventItem = await _repository.GetByIdAsync(id);

                if (eventItem == null)
                {
                    return false;
                }

                await _repository.DeleteAsync(eventItem);

                return true;
            }
}