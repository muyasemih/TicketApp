using Microsoft.EntityFrameworkCore;
using TicketApp.Models;

namespace TicketApp.Repositories;

public class VenueRepository : IVenueRepository
{
    private readonly AppDbContext _context;

    public VenueRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Venue>> GetAllAsync()
    {
        return await _context.Venues
            .Include(v => v.Blocks)
            .ToListAsync();
    }

    public async Task<Venue?> GetByIdAsync(int id)
    {
        return await _context.Venues
            .Include(v => v.Blocks)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Venue> CreateAsync(Venue venue)
    {
        _context.Venues.Add(venue);

        await _context.SaveChangesAsync();

        return venue;
    }

    public async Task<Venue?> UpdateAsync(int id, Venue venue)
    {
        var existingVenue = await _context.Venues
            .Include(v => v.Blocks)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (existingVenue == null)
        {
            return null;
        }

        existingVenue.Name = venue.Name;

        _context.VenueBlocks.RemoveRange(existingVenue.Blocks);

        existingVenue.Blocks = venue.Blocks;

        await _context.SaveChangesAsync();

        return existingVenue;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var venue = await _context.Venues
            .Include(v => v.Blocks)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venue == null)
        {
            return false;
        }

        _context.Venues.Remove(venue);

        await _context.SaveChangesAsync();

        return true;
    }
}