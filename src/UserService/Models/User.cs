namespace UserService.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public record CreateUserRequest(string Name, string Email, string Password, string Role = "Customer");
public record UpdateUserRequest(string? Name, string? Email);
public record AuthenticateRequest(string Email, string Password);
public record AuthenticateResponse(bool Success, Guid? UserId, string? Name, string? Role, string? Token);
