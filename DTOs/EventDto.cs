namespace TicketApp.DTOs;

public class EventDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime EventDate { get; set; }

    public int? VenueId { get; set; }

    public VenueDto? Venue { get; set; }

    public List<EventBlockPriceDto> BlockPrices { get; set; } = new();
}