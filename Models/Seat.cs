using System.Text.Json.Serialization;

namespace TicketApp.Models;

public class Seat
{
    public int Id { get; set; }

    public int RowNumber { get; set; }

    public int SeatNumber { get; set; }

    public int Number => RowNumber * 100 + SeatNumber;

    public int VenueBlockId { get; set; }

    [JsonIgnore]
    public VenueBlock VenueBlock { get; set; } = null!;
}