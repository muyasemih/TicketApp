using System.ComponentModel.DataAnnotations;

namespace TicketApp.DTOs;

public class UpdateEventDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime? EventDate { get; set; }

    public int? VenueId { get; set; }

    public List<UpdateEventBlockPriceDto> Blocks { get; set; } = new();
}

public class UpdateEventBlockPriceDto
{
    [Range(1, int.MaxValue)]
    public int VenueBlockId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
}