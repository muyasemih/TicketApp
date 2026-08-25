using System.ComponentModel.DataAnnotations;

namespace TicketApp.DTOs;

public class UpdateEventDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    public int? VenueId { get; set; }
}