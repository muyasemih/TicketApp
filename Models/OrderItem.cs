namespace TicketApp.Models;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public Order Order { get; set; } = null!;

    public int EventSeatId { get; set; }

    public EventSeat EventSeat { get; set; } = null!;

    public decimal Price { get; set; }

    public Ticket? Ticket { get; set; }
}