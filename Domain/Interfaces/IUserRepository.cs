using OpenClawApi.Domain.Entities;

namespace OpenClawApi.Domain.Interfaces;

public interface IUserRepository
{
    Task<User> AddAsync(User user);
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
}
