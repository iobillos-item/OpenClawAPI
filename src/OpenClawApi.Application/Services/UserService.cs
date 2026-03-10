using OpenClawApi.Application.DTOs;
using OpenClawApi.Application.Interfaces;
using OpenClawApi.Domain.Entities;
using OpenClawApi.Domain.Interfaces;

namespace OpenClawApi.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        var existingUser = await _userRepository.GetByEmailAsync(createUserDto.Email);
        if (existingUser is not null)
            throw new InvalidOperationException("User with this email already exists");

        var user = new User
        {
            Username = createUserDto.Username,
            Email = createUserDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password),
            CreatedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepository.AddAsync(user);
        return MapToDto(createdUser);
    }

    public async Task<IReadOnlyList<UserResponseDto>> GetUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToDto).ToList();
    }

    public async Task<UserResponseDto> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new InvalidOperationException("User not found");

        return MapToDto(user);
    }

    public async Task<UserResponseDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new InvalidOperationException("User not found");

        var existingUserWithEmail = await _userRepository.GetByEmailAsync(updateUserDto.Email);
        if (existingUserWithEmail is not null && existingUserWithEmail.Id != id)
            throw new InvalidOperationException("User with this email already exists");

        user.Username = updateUserDto.Username;
        user.Email = updateUserDto.Email;

        var updatedUser = await _userRepository.UpdateAsync(user);
        return MapToDto(updatedUser);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new InvalidOperationException("User not found");

        return await _userRepository.DeleteAsync(id);
    }

    private static UserResponseDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        CreatedAt = user.CreatedAt
    };
}
