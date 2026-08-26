namespace TicketApp.Models;

public class Event
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int? VenueId { get; set; }
    public DateTime EventDate { get; set; }

    public Venue? Venue { get; set; }
}