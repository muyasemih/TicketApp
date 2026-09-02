using System.Text.Json.Serialization;

namespace TicketApp.Models;

public class EventSeat
{
    public int Id { get; set; }

    public int EventId { get; set; }

    [JsonIgnore]
    public Event Event { get; set; } = null!;

    public int SeatId { get; set; }

    [JsonIgnore]
    public Seat Seat { get; set; } = null!;

    public EventSeatStatus Status { get; set; }

    public DateTime? ReservedUntil { get; set; }

    public int? ReservedByUserId { get; set; }

    [JsonIgnore]
    public User? ReservedByUser { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}