namespace TicketApp.Models;

public class User
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;
    
    public bool IsStudent { get; set; } = false;

    public string Role { get; set; } = "User";
}