using UserService.Models;

namespace UserService.Repositories;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task<User?> UpdateAsync(Guid id, Action<User> update);
    Task<bool> DeleteAsync(Guid id);
}

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users =
    [
        new() { Id = Guid.Parse("11111111-0000-0000-0000-000000000001"), Name = "Alice Johnson", Email = "alice@example.com", PasswordHash = "hashed_pass1", Role = "Customer" },
        new() { Id = Guid.Parse("11111111-0000-0000-0000-000000000002"), Name = "Bob Smith",    Email = "bob@example.com",   PasswordHash = "hashed_pass2", Role = "Customer" },
        new() { Id = Guid.Parse("11111111-0000-0000-0000-000000000003"), Name = "Carol White",  Email = "carol@example.com", PasswordHash = "hashed_pass3", Role = "Admin" },
    ];

    public Task<List<User>> GetAllAsync() => Task.FromResult(_users.Where(u => u.IsActive).ToList());

    public Task<User?> GetByIdAsync(Guid id) => Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

    public Task<User?> GetByEmailAsync(string email) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));

    public Task<User> CreateAsync(User user)
    {
        _users.Add(user);
        return Task.FromResult(user);
    }

    public Task<User?> UpdateAsync(Guid id, Action<User> update)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user is null) return Task.FromResult<User?>(null);
        update(user);
        user.UpdatedAt = DateTime.UtcNow;
        return Task.FromResult<User?>(user);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user is null) return Task.FromResult(false);
        user.IsActive = false;
        return Task.FromResult(true);
    }
}
