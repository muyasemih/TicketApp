using Microsoft.EntityFrameworkCore;
using TicketApp.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Event> Events { get; set; }
    public DbSet<Venue> Venues { get; set; }
    public DbSet<VenueBlock> VenueBlocks { get; set; }
    public DbSet<Seat> Seats { get; set; }
    public DbSet<EventSeat> EventSeats { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>()
            .Property(e => e.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Seat>()
            .HasIndex(s => new
            {
                s.VenueBlockId,
                s.RowNumber,
                s.SeatNumber
            })
            .IsUnique();

        modelBuilder.Entity<Event>()
            .HasOne(e => e.Venue)
            .WithMany()
            .HasForeignKey(e => e.VenueId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EventSeat>()
            .HasOne(es => es.Event)
            .WithMany()
            .HasForeignKey(es => es.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EventSeat>()
            .HasOne(es => es.Seat)
            .WithMany()
            .HasForeignKey(es => es.SeatId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EventSeat>()
            .HasIndex(es => new
            {
                es.EventId,
                es.SeatId
            })
            .IsUnique();
    }
}