namespace TicketApp.Models;

public enum TicketStatus
{
    Active = 1,
    Cancelled = 2
}

public class Ticket
{
    public int Id { get; set; }

    public int OrderItemId { get; set; }

    public OrderItem OrderItem { get; set; } = null!;

    public string TicketNumber { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public TicketStatus Status { get; set; } = TicketStatus.Active;

    public DateTime? CancelledAt { get; set; }
}