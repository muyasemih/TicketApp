using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using TicketApp.DTOs;
using TicketApp.Models;
using TicketApp.Repositories;

namespace TicketApp.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _configuration;

    public UserService(
        IUserRepository repository,
        IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<UserDto> CreateAsync(CreateUserDto newUser)
    {
        var email = newUser.Email.Trim().ToLowerInvariant();

        var existingUser = await _repository.GetByEmailAsync(email);

        if (existingUser != null)
        {
            throw new ArgumentException(
                "Bu email adresi zaten kayıtlı.");
        }

        var user = new User
            {
                Name = newUser.Name.Trim(),
                Email = email,
                IsStudent = newUser.IsStudent
            };

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            newUser.Password);

            await _repository.CreateAsync(user);

            var createdUser = await _repository.GetByEmailAsync(email);

            return new UserDto
            {
                Id = createdUser!.Id,
                Name = createdUser.Name,
                Email = createdUser.Email,
                IsStudent = createdUser.IsStudent
            };
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginUserDto loginUser)
    {
        var email = loginUser.Email.Trim().ToLowerInvariant();

        var user = await _repository.GetByEmailAsync(email);

        if (user == null)
        {
            return null;
        }

        var passwordResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            loginUser.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var token = GenerateToken(user);

        return new LoginResponseDto
        {
            Token = token,
          User = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                IsStudent = user.IsStudent
            }
        };
    }

    private string GenerateToken(User user)
    {
        var jwtKey = _configuration["Jwt:Key"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException(
                "Jwt:Key appsettings.json içinde bulunamadı.");
        }

        var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    user.Name),

                new Claim(
                    ClaimTypes.Email,
                    user.Email),

                new Claim(
                    ClaimTypes.Role,
                    user.Role)
            };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var expiresInMinutes =
            _configuration.GetValue<int>("Jwt:ExpiresInMinutes");

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}