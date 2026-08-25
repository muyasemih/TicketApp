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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>()
            .Property(e => e.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Event>()
            .HasOne(e => e.Venue)
            .WithMany()
            .HasForeignKey(e => e.VenueId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}