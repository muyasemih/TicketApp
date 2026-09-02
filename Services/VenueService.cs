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
        ValidateBlocks(venue);

        GenerateSeats(venue);

        return await _repository.CreateAsync(venue);
    }

    public async Task<Venue?> UpdateAsync(int id, Venue venue)
    {
        ValidateBlocks(venue);

        GenerateSeats(venue);

        return await _repository.UpdateAsync(id, venue);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    private void ValidateBlocks(Venue venue)
    {
        foreach (var block in venue.Blocks)
        {
            if (block.Type == VenueBlockType.Seated)
            {
                if (block.RowCount <= 0)
                {
                    throw new ArgumentException(
                        $"'{block.Name}' için RowCount 0'dan büyük olmalıdır.");
                }

                if (block.SeatsPerRow <= 0)
                {
                    throw new ArgumentException(
                        $"'{block.Name}' için SeatsPerRow 0'dan büyük olmalıdır.");
                }
            }
            else if (block.Type == VenueBlockType.Standing)
            {
                if (block.Capacity <= 0)
                {
                    throw new ArgumentException(
                        $"'{block.Name}' için Capacity 0'dan büyük olmalıdır.");
                }

                block.RowCount = 0;
                block.SeatsPerRow = 0;
            }
        }
    }

    private void GenerateSeats(Venue venue)
{
    foreach (var block in venue.Blocks)
    {
        if (block.Type == VenueBlockType.Seated)
        {
            block.Capacity = block.RowCount * block.SeatsPerRow;

            block.Seats = new List<Seat>();

            for (int row = 1; row <= block.RowCount; row++)
            {
                for (int seat = 1; seat <= block.SeatsPerRow; seat++)
                {
                    block.Seats.Add(new Seat
                    {
                        RowNumber = row,
                        SeatNumber = seat
                    });
                }
            }
        }
        else if (block.Type == VenueBlockType.Standing)
        {
            block.Seats = new List<Seat>();

            for (int i = 1; i <= block.Capacity; i++)
            {
                block.Seats.Add(new Seat
                {
                    RowNumber = 0,
                    SeatNumber = i
                });
            }
        }
    }
    }
}