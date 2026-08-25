namespace TicketApp.Models;

public class Venue
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<VenueBlock> Blocks { get; set; } = new();
}