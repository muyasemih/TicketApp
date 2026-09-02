using System.Text.Json.Serialization;

namespace TicketApp.Models;

public class EventBlockPrice
{
    public int Id { get; set; }

    public int EventId { get; set; }

    [JsonIgnore]
    public Event Event { get; set; } = null!;

    public int VenueBlockId { get; set; }

    [JsonIgnore]
    public VenueBlock VenueBlock { get; set; } = null!;

    public decimal Price { get; set; }
}