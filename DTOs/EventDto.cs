namespace TicketApp.DTOs;

public class EventDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int? VenueId { get; set; }

    public VenueDto? Venue { get; set; }
}