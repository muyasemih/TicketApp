namespace TicketApp.Models;

public class Ticket
{
    public int Id { get; set; }

    public int OrderItemId { get; set; }

    public OrderItem OrderItem { get; set; } = null!;

    public string TicketNumber { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}