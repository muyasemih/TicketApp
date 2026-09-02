using TicketApp.Models;

namespace TicketApp.DTOs;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;

    public UserDto User { get; set; } = null!;
}