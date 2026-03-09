using OpenClawApi.Application.DTOs;

namespace OpenClawApi.Application.Interfaces;

public interface IUserService
{
    Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto);
    Task<IReadOnlyList<UserResponseDto>> GetUsersAsync();
    Task<UserResponseDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
    Task<bool> DeleteUserAsync(int id);
}
    