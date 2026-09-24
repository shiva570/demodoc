using Shared.Contracts;
using UserService.Models;
using UserService.Repositories;

namespace UserService.Services;

public interface IUserService
{
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(Guid id);
    Task<UserDto> CreateUserAsync(CreateUserRequest request);
    Task<UserDto?> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(Guid id);
    Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest request);
}

public class UserDomainService : IUserService
{
    private readonly IUserRepository _repository;

    public UserDomainService(IUserRepository repository) => _repository = repository;

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _repository.GetAllAsync();
        return users.Select(ToDto).ToList();
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _repository.GetByIdAsync(id);
        return user is null ? null : ToDto(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = $"hashed_{request.Password}",
            Role = request.Role
        };
        var created = await _repository.CreateAsync(user);
        return ToDto(created);
    }

    public async Task<UserDto?> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _repository.UpdateAsync(id, u =>
        {
            if (request.Name is not null) u.Name = request.Name;
            if (request.Email is not null) u.Email = request.Email;
        });
        return user is null ? null : ToDto(user);
    }

    public Task<bool> DeleteUserAsync(Guid id) => _repository.DeleteAsync(id);

    public async Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest request)
    {
        var user = await _repository.GetByEmailAsync(request.Email);
        if (user is null || user.PasswordHash != $"hashed_{request.Password}")
            return new AuthenticateResponse(false, null, null, null, null);

        var token = $"jwt_token_{user.Id}_{DateTime.UtcNow.Ticks}";
        return new AuthenticateResponse(true, user.Id, user.Name, user.Role, token);
    }

    private static UserDto ToDto(User u) => new(u.Id, u.Name, u.Email, u.CreatedAt);
}
