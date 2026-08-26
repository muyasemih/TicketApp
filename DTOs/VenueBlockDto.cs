using TicketApp.Models;

namespace TicketApp.DTOs;

public class VenueBlockDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public VenueBlockType Type { get; set; }

    public int RowCount { get; set; }

    public int SeatsPerRow { get; set; }

    public int Capacity { get; set; }
}