using Microsoft.AspNetCore.Mvc;
using TicketApp.DTOs;
using TicketApp.Services;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserDto newUser)
    {
        var user = await _service.CreateAsync(newUser);

        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserDto loginUser)
    {
        var user = await _service.LoginAsync(loginUser);

        if (user == null)
        {
            return Unauthorized(new
            {
                error = "Email veya şifre hatalı."
            });
        }

        return Ok(user);
    }
}