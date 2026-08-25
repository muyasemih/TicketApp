using System.ComponentModel.DataAnnotations;

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

    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }
}