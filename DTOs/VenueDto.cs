namespace TicketApp.DTOs;

public class VenueDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<VenueBlockDto> Blocks { get; set; } = new();
}