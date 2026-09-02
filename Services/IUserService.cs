using TicketApp.DTOs;

namespace TicketApp.Services;

public interface IUserService
{
    Task<UserDto> CreateAsync(CreateUserDto newUser);

    Task<LoginResponseDto?> LoginAsync(LoginUserDto loginUser);
}