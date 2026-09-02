using Microsoft.EntityFrameworkCore;
using TicketApp.DTOs;
using TicketApp.Models;
using TicketApp.Repositories;

namespace TicketApp.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly AppDbContext _db;

    public OrderService(
        IOrderRepository orderRepository,
        AppDbContext db,
        IHttpContextAccessor httpContextAccessor)
    {
        _orderRepository = orderRepository;
        _db = db;
    }

    public async Task<OrderDto?> CreateAsync(
        int userId,
        CreateOrderDto newOrder)
        
    {
        var user = await _db.Users.FindAsync(userId);

            if (user == null)
            {
                return null;
            }
        var eventSeats = await _orderRepository.GetEventSeatsAsync(
            newOrder.EventId,
            newOrder.EventSeatIds);

        if (eventSeats.Count != newOrder.EventSeatIds.Count)
        {
            return null;
        }

        var now = DateTime.UtcNow;

        foreach (var eventSeat in eventSeats)
        {
            if (eventSeat.Status != EventSeatStatus.Reserved)
            {
                return null;
            }

            if (eventSeat.ReservedByUserId != userId)
            {
                return null;
            }

            if (!eventSeat.ReservedUntil.HasValue ||
                eventSeat.ReservedUntil.Value <= now)
            {
                return null;
            }
        }

        var blockPrices = await _db.EventBlockPrices
            .Where(bp =>
                bp.EventId == newOrder.EventId &&
                eventSeats
                    .Select(es => es.Seat.VenueBlockId)
                    .Contains(bp.VenueBlockId))
            .ToListAsync();

        var orderItems = new List<OrderItem>();

        foreach (var eventSeat in eventSeats)
        {
            var blockPrice = blockPrices.FirstOrDefault(bp =>
                bp.VenueBlockId == eventSeat.Seat.VenueBlockId);

            if (blockPrice == null)
            {
                return null;
            }

            orderItems.Add(new OrderItem
            {
                EventSeatId = eventSeat.Id,
                Price = blockPrice.Price
            });
        }

        var totalAmount = orderItems.Sum(item => item.Price);

            if (user.IsStudent)
            {
                totalAmount *= 0.90m;
            }
        var order = new Order
        {
            UserId = userId,
            CreatedAt = now,
            TotalAmount = totalAmount,
            Items = orderItems
        };

        foreach (var eventSeat in eventSeats)
        {
            eventSeat.Status = EventSeatStatus.Sold;
            eventSeat.ReservedUntil = null;
            eventSeat.ReservedByUserId = null;
        }

        await using var transaction =
            await _db.Database.BeginTransactionAsync();

        try
        {
            await _orderRepository.CreateAsync(order);

            await _db.SaveChangesAsync();

            var createdTickets = new List<Ticket>();

            foreach (var orderItem in order.Items)
            {
                var ticket = new Ticket
                {
                    OrderItemId = orderItem.Id,
                    TicketNumber = $"TKT-{Guid.NewGuid():N}".ToUpper(),
                    CreatedAt = now
                };

                createdTickets.Add(ticket);
            }

            _db.Tickets.AddRange(createdTickets);

            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                Items = order.Items.Select(item =>
                {
                    var ticket = createdTickets.First(
                        t => t.OrderItemId == item.Id);

                    return new OrderItemDto
                    {
                        Id = item.Id,
                        EventSeatId = item.EventSeatId,
                        Price = item.Price,
                        TicketId = ticket.Id,
                        TicketNumber = ticket.TicketNumber
                    };
                }).ToList()
            };
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();

            return null;
        }
    }

    public async Task<List<OrderDto>> GetByUserIdAsync(int userId)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId);

        return orders.Select(order => new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(item => new OrderItemDto
            {
                Id = item.Id,
                EventSeatId = item.EventSeatId,
                Price = item.Price,
                TicketId = item.Ticket?.Id ?? 0,
                TicketNumber = item.Ticket?.TicketNumber ?? string.Empty
            }).ToList()
        }).ToList();
    }
}