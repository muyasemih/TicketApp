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

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return null;
        }

        const decimal studentDiscountRate = 0.10m;

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
                Price = user.IsStudent
                    ? Math.Round(blockPrice.Price * (1 - studentDiscountRate), 2)
                    : blockPrice.Price
            });
        }

        var order = new Order
        {
            UserId = userId,
            CreatedAt = now,
            TotalAmount = orderItems.Sum(item => item.Price),
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
                TicketNumber = item.Ticket?.TicketNumber ?? string.Empty,
                Status = item.Ticket?.Status.ToString() ?? "Active",
                CancelledAt = item.Ticket?.CancelledAt
            }).ToList()
        }).ToList();
    }
    public async Task<(bool Success, string Message)> CancelTicketAsync(int ticketId, int userId, bool isAdmin = false)
    {
        var ticket = await _db.Tickets
            .Include(t => t.OrderItem)
                .ThenInclude(oi => oi.Order)
            .Include(t => t.OrderItem)
                .ThenInclude(oi => oi.EventSeat)
                    .ThenInclude(es => es.Event)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
        {
            return (false, "Bilet bulunamadı.");
        }

        if (!isAdmin && ticket.OrderItem.Order.UserId != userId)
        {
            return (false, "Bu bileti iptal etme yetkiniz yok.");
        }

        if (ticket.Status == TicketStatus.Cancelled)
        {
            return (false, "Bu bilet zaten iptal edilmiş.");
        }

        var eventDate = ticket.OrderItem.EventSeat.Event.EventDate;
        var now = DateTime.UtcNow;

        if (eventDate <= now)
        {
            return (false, "Geçmiş etkinlikler için bilet iptali yapılamaz.");
        }

        if (eventDate - now < TimeSpan.FromHours(2))
        {
            return (false, "Etkinliğin başlamasına 2 saatten az bir süre kaldığı için bilet iptal edilemez.");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            ticket.Status = TicketStatus.Cancelled;
            ticket.CancelledAt = now;

            var eventSeat = ticket.OrderItem.EventSeat;
            eventSeat.Status = EventSeatStatus.Available;
            eventSeat.ReservedUntil = null;
            eventSeat.ReservedByUserId = null;

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, "Bilet başarıyla iptal edildi ve koltuk tekrar satışa açıldı.");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return (false, "İptal işlemi sırasında bir hata oluştu.");
        }
    }
}