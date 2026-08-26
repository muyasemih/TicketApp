using System.ComponentModel.DataAnnotations;
using TicketApp.Models;

namespace TicketApp.DTOs;

public class CreateVenueDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public List<CreateVenueBlockDto> Blocks { get; set; } = new();
}

public class CreateVenueBlockDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public VenueBlockType Type { get; set; }

    public int RowCount { get; set; }

    public int SeatsPerRow { get; set; }

    public int Capacity { get; set; }
}