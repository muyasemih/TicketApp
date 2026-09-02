using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketApp.DTOs;
using TicketApp.Services;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _service;

    public OrdersController(IOrderService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderDto newOrder)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new
            {
                error = "Kullanıcı kimliği doğrulanamadı."
            });
        }

        var order = await _service.CreateAsync(userId, newOrder);

        if (order == null)
        {
            return Conflict(new
            {
                error = "Sipariş oluşturulamadı. Seçilen koltuk artık kullanılamıyor."
            });
        }

        return Ok(order);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyOrders()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new
            {
                error = "Kullanıcı kimliği doğrulanamadı."
            });
        }

        var orders = await _service.GetByUserIdAsync(userId);

        return Ok(orders);
    }
}