using TicketApp.DTOs;

namespace TicketApp.Services;

public interface IOrderService
{
    Task<OrderDto?> CreateAsync(int userId, CreateOrderDto newOrder);
    Task<List<OrderDto>> GetByUserIdAsync(int userId);
    Task<(bool Success, string Message)> CancelTicketAsync(int ticketId, int userId, bool isAdmin = false);
}