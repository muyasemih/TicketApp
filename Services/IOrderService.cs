using TicketApp.DTOs;

namespace TicketApp.Services;

public interface IOrderService
{
    Task<OrderDto?> CreateAsync(int userId, CreateOrderDto newOrder);

    Task<List<OrderDto>> GetByUserIdAsync(int userId);
}