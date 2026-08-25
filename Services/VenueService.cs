using TicketApp.Models;
using TicketApp.Repositories;

namespace TicketApp.Services;

public class VenueService : IVenueService
{
    private readonly IVenueRepository _repository;

    public VenueService(IVenueRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Venue>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Venue?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Venue> CreateAsync(Venue venue)
    {
        return await _repository.CreateAsync(venue);
    }

    public async Task<Venue?> UpdateAsync(int id, Venue venue)
    {
        return await _repository.UpdateAsync(id, venue);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }
}