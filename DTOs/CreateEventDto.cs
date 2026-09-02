using System.ComponentModel.DataAnnotations;

namespace TicketApp.DTOs;

public class CreateEventDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime? EventDate { get; set; }

    public int? VenueId { get; set; }

    public List<CreateEventBlockPriceDto> Blocks { get; set; } = new();
}

public class CreateEventBlockPriceDto
{
    [Range(1, int.MaxValue)]
    public int VenueBlockId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
}