namespace TicketApp.Models;

public class EventSeat
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public Event Event { get; set; } = null!;

    public int SeatId { get; set; }

    public Seat Seat { get; set; } = null!;

    public EventSeatStatus Status { get; set; }
}