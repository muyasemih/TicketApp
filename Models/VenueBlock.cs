using System.Text.Json.Serialization;

namespace TicketApp.Models;

public class VenueBlock
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public int VenueId { get; set; }

    [JsonIgnore]
    public Venue Venue { get; set; } = null!;
}