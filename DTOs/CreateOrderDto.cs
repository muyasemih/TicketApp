using System.ComponentModel.DataAnnotations;

namespace TicketApp.DTOs;

public class CreateOrderDto
{
    [Required]
    public int EventId { get; set; }

    [Required]
    [MinLength(1)]
    public List<int> EventSeatIds { get; set; } = new();
}