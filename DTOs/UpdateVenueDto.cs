using System.ComponentModel.DataAnnotations;
using TicketApp.Models;

namespace TicketApp.DTOs;

public class UpdateVenueDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public List<UpdateVenueBlockDto> Blocks { get; set; } = new();
}

public class UpdateVenueBlockDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public VenueBlockType Type { get; set; }

    [Range(1, int.MaxValue)]
    public int RowCount { get; set; }

    [Range(1, int.MaxValue)]
    public int SeatsPerRow { get; set; }

    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }
}