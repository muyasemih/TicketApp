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
            .AsNoTracking()
            .Include(v => v.Blocks)
            .ToListAsync();
    }

    public async Task<Venue?> GetByIdAsync(int id)
    {
        return await _context.Venues
            .AsNoTracking()
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

        var existingBlockIds = existingVenue.Blocks.Select(b => b.Id).ToList();
        var isReferencedByEvents = await _context.EventBlockPrices
            .AnyAsync(ebp => existingBlockIds.Contains(ebp.VenueBlockId));

        if (!isReferencedByEvents)
        {
            _context.VenueBlocks.RemoveRange(existingVenue.Blocks);
            existingVenue.Blocks = venue.Blocks;
        }
        else
        {
            // Fiyat veya etkinlik bağlantısı varsa blok ID'lerini koru, sadece isim ve kapasiteyi güncelle
            foreach (var incomingBlock in venue.Blocks)
            {
                var match = existingVenue.Blocks.FirstOrDefault(b => b.Id == incomingBlock.Id || b.Name == incomingBlock.Name);
                if (match != null)
                {
                    match.Name = incomingBlock.Name;
                    match.Type = incomingBlock.Type;
                    match.RowCount = incomingBlock.RowCount;
                    match.SeatsPerRow = incomingBlock.SeatsPerRow;
                    match.Capacity = incomingBlock.Capacity;
                }
                else
                {
                    existingVenue.Blocks.Add(incomingBlock);
                }
            }
        }

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

        var hasEvents = await _context.Events.AnyAsync(e => e.VenueId == id);
        if (hasEvents)
        {
            throw new InvalidOperationException("Bu salona tanımlı etkinlikler olduğu için salon silinemez.");
        }

        _context.Venues.Remove(venue);

        await _context.SaveChangesAsync();

        return true;
    }
}