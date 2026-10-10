using System.ComponentModel.DataAnnotations;

namespace TicketApp.DTOs;

public class UpdateProfileDto
{
    [Required]
    [MinLength(2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
}
