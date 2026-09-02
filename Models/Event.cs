namespace TicketApp.Models;

public class Event
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    
    public int? VenueId { get; set; }
    public DateTime EventDate { get; set; }

    public Venue? Venue { get; set; }
    public List<EventBlockPrice> BlockPrices { get; set; } = new();
}