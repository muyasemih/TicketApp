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
    [HttpPost("tickets/{ticketId:int}/cancel")]
    public async Task<IActionResult> CancelTicket(int ticketId)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var isAdmin = User.IsInRole("Admin");

        var result = await _service.CancelTicketAsync(ticketId, userId, isAdmin);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(new { message = result.Message });
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