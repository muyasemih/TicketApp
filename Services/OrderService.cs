using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TicketApp.DTOs;
using TicketApp.Models;
using TicketApp.Repositories;

namespace TicketApp.Services;

public class OrderService : IOrderService
{
    private const decimal StudentDiscountRate = 0.10m;

    private readonly IOrderRepository _orderRepository;
    private readonly AppDbContext _db;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        AppDbContext db,
        IPaymentService paymentService,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _db = db;
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<OrderDto?> CreateAsync(
        int userId,
        CreateOrderDto newOrder)
    {
        if (newOrder.EventSeatIds is null || newOrder.EventSeatIds.Count == 0)
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

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return null;
        }

        var blockIds = eventSeats
            .Select(es => es.Seat.VenueBlockId)
            .Distinct()
            .ToList();

        var blockPrices = await _db.EventBlockPrices
            .Where(bp =>
                bp.EventId == newOrder.EventId &&
                blockIds.Contains(bp.VenueBlockId))
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
                    ? Math.Round(blockPrice.Price * (1 - StudentDiscountRate), 2)
                    : blockPrice.Price
            });
        }

        var totalAmount = orderItems.Sum(item => item.Price);

        var cleanCard = newOrder.Payment.CardNumber.Replace(" ", "").Replace("-", "");
        var lastFour = cleanCard.Length >= 4 ? cleanCard[^4..] : cleanCard;

        var order = new Order
        {
            UserId = userId,
            CreatedAt = now,
            TotalAmount = totalAmount,
            PaymentStatus = PaymentStatus.Paid,
            CardLastFourDigits = lastFour,
            Items = orderItems
        };

        await using var transaction = await _db.Database.BeginTransactionAsync();
        string? chargedTransactionId = null;

        try
        {
            foreach (var eventSeat in eventSeats)
            {
                eventSeat.Status = EventSeatStatus.Sold;
                eventSeat.ReservedUntil = null;
                eventSeat.ReservedByUserId = null;
            }

            await _orderRepository.CreateAsync(order);
            await _db.SaveChangesAsync();

            var paymentResult = await _paymentService.ProcessPaymentAsync(totalAmount, newOrder.Payment);

            if (!paymentResult.IsSuccess)
            {
                await transaction.RollbackAsync();
                return null;
            }

            chargedTransactionId = paymentResult.TransactionId;
            order.PaymentTransactionId = chargedTransactionId;

            var createdTickets = order.Items
                .Select(item => new Ticket
                {
                    OrderItemId = item.Id,
                    TicketNumber = $"TKT-{Guid.NewGuid():N}".ToUpper(),
                    CreatedAt = now
                })
                .ToList();

            _db.Tickets.AddRange(createdTickets);
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            return ToOrderDto(order, createdTickets);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex,
                "Eşzamanlılık çakışması. UserId: {UserId}, EventId: {EventId}",
                userId, newOrder.EventId);

            await RollbackAndRefundAsync(transaction, chargedTransactionId, totalAmount);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Sipariş oluşturulurken beklenmeyen hata. UserId: {UserId}, EventId: {EventId}",
                userId, newOrder.EventId);

            await RollbackAndRefundAsync(transaction, chargedTransactionId, totalAmount);
            throw;
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
                    .ThenInclude(o => o.Items)
                        .ThenInclude(i => i.Ticket)
            .Include(t => t.OrderItem)
                .ThenInclude(oi => oi.EventSeat)
                    .ThenInclude(es => es.Event)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
        {
            return (false, "Bilet bulunamadı.");
        }

        var order = ticket.OrderItem.Order;

        if (!isAdmin && order.UserId != userId)
        {
            return (false, "Bu bileti iptal etme yetkiniz yok.");
        }

        if (ticket.Status == TicketStatus.Cancelled)
        {
            return (false, "Bu bilet zaten iptal edilmiş.");
        }

        var rawEventDate = ticket.OrderItem.EventSeat.Event.EventDate;
        var eventDate = rawEventDate.Kind == DateTimeKind.Utc
            ? rawEventDate
            : DateTime.SpecifyKind(rawEventDate, DateTimeKind.Utc);
        var now = DateTime.UtcNow;

        if (eventDate <= now)
        {
            return (false, "Geçmiş etkinlikler için bilet iptali yapılamaz.");
        }

        if (eventDate - now < TimeSpan.FromHours(2))
        {
            return (false, "Etkinliğin başlamasına 2 saatten az bir süre kaldığı için bilet iptal edilemez.");
        }

        var refundAmount = ticket.OrderItem.Price;
        var transactionId = order.PaymentTransactionId ?? "MOCK-TXN";

        await using var transaction = await _db.Database.BeginTransactionAsync();
        var refunded = false;

        try
        {
            ticket.Status = TicketStatus.Cancelled;
            ticket.CancelledAt = now;

            var eventSeat = ticket.OrderItem.EventSeat;
            eventSeat.Status = EventSeatStatus.Available;
            eventSeat.ReservedUntil = null;
            eventSeat.ReservedByUserId = null;

            var allTickets = order.Items.Select(i => i.Ticket).Where(t => t != null).ToList();
            var allCancelled = allTickets.All(t => t!.Id == ticket.Id || t.Status == TicketStatus.Cancelled);

            order.PaymentStatus = allCancelled ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded;

            await _db.SaveChangesAsync();

            var refundResult = await _paymentService.ProcessRefundAsync(transactionId, refundAmount);

            if (!refundResult.IsSuccess)
            {
                await transaction.RollbackAsync();
                return (false, $"İade işlemi kart sağlayıcısı tarafından reddedildi: {refundResult.ErrorMessage}");
            }

            refunded = true;
            await transaction.CommitAsync();

            var cardInfo = !string.IsNullOrEmpty(order.CardLastFourDigits)
                ? $"**** **** **** {order.CardLastFourDigits}"
                : "kartınıza";

            return (true, $"Bilet iptal edildi. {refundAmount:N2} TL tutarındaki ücret {cardInfo} nolu karta iade edildi. Referans: {refundResult.TransactionId}");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Bilet iptalinde eşzamanlılık çakışması. TicketId: {TicketId}", ticketId);
            await transaction.RollbackAsync();
            return (false, "Bu bilet için başka bir işlem zaten yapılıyor veya tamamlandı. Lütfen sayfayı yenileyin.");
        }
        catch (Exception ex)
        {
            if (refunded)
            {
                _logger.LogCritical(ex,
                    "İade yapıldı ancak iptal kaydedilemedi! TicketId: {TicketId}, Tutar: {Amount}, İşlem: {TransactionId}",
                    ticketId, refundAmount, transactionId);
            }
            else
            {
                _logger.LogError(ex, "Bilet iptali sırasında hata. TicketId: {TicketId}", ticketId);
            }

            await transaction.RollbackAsync();
            return (false, "İptal işlemi sırasında bir hata oluştu.");
        }
    }

    private async Task RollbackAndRefundAsync(
        IDbContextTransaction transaction,
        string? chargedTransactionId,
        decimal amount)
    {
        try
        {
            await transaction.RollbackAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction geri alınamadı.");
        }

        if (chargedTransactionId is null)
        {
            return;
        }

        try
        {
            var refund = await _paymentService.ProcessRefundAsync(chargedTransactionId, amount);

            if (!refund.IsSuccess)
            {
                _logger.LogCritical(
                    "Ödeme alındı, sipariş kaydedilemedi ve iade reddedildi! İşlem: {TransactionId}, Tutar: {Amount}, Hata: {Error}",
                    chargedTransactionId, amount, refund.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex,
                "Ödeme alındı, sipariş kaydedilemedi ve iade sırasında hata oluştu! İşlem: {TransactionId}, Tutar: {Amount}",
                chargedTransactionId, amount);
        }
    }

    private static OrderDto ToOrderDto(Order order, List<Ticket> tickets)
    {
        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(item =>
            {
                var ticket = tickets.First(t => t.OrderItemId == item.Id);

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
}