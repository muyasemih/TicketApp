namespace TicketApp.DTOs;

public class OrderItemDto
{
    public int Id { get; set; }

    public int EventSeatId { get; set; }

    public decimal Price { get; set; }

    public int TicketId { get; set; }

    public string TicketNumber { get; set; } = string.Empty;
}