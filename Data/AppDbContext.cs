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
    public DbSet<User> Users { get; set; }
    public DbSet<EventBlockPrice> EventBlockPrices { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Ticket> Tickets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

        modelBuilder.Entity<EventSeat>()
            .Property(es => es.RowVersion)
            .IsRowVersion();

        modelBuilder.Entity<EventSeat>()
            .HasOne(es => es.ReservedByUser)
            .WithMany()
            .HasForeignKey(es => es.ReservedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<EventBlockPrice>()
            .HasOne(ebp => ebp.Event)
            .WithMany(e => e.BlockPrices)
            .HasForeignKey(ebp => ebp.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EventBlockPrice>()
            .HasOne(ebp => ebp.VenueBlock)
            .WithMany()
            .HasForeignKey(ebp => ebp.VenueBlockId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EventBlockPrice>()
            .HasIndex(ebp => new
            {
                ebp.EventId,
                ebp.VenueBlockId
            })
            .IsUnique();

        modelBuilder.Entity<EventBlockPrice>()
            .Property(ebp => ebp.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasDefaultValue("User");

        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.EventSeat)
            .WithMany()
            .HasForeignKey(oi => oi.EventSeatId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.OrderItem)
            .WithOne(oi => oi.Ticket)
            .HasForeignKey<Ticket>(t => t.OrderItemId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ticket>()
            .HasIndex(t => t.TicketNumber)
            .IsUnique();
    }
}