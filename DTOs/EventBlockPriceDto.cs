namespace TicketApp.DTOs;

public class EventBlockPriceDto
{
    public int Id { get; set; }

    public int VenueBlockId { get; set; }

    public decimal Price { get; set; }
}