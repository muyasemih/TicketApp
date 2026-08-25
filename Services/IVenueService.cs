using TicketApp.Models;

namespace TicketApp.Services;

public interface IVenueService
{
    Task<List<Venue>> GetAllAsync();
    Task<Venue?> GetByIdAsync(int id);
    Task<Venue> CreateAsync(Venue venue);
    Task<Venue?> UpdateAsync(int id, Venue venue);
    Task<bool> DeleteAsync(int id);
}