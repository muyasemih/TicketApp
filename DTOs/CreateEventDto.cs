using System.ComponentModel.DataAnnotations;

namespace TicketApp.DTOs;

public class CreateEventDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
}