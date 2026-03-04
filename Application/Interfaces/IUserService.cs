using OpenClawApi.Application.DTOs;

namespace OpenClawApi.Application.Interfaces;

public interface IUserService
{
    Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto);
}
