using System.ComponentModel.DataAnnotations;

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

    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }
}