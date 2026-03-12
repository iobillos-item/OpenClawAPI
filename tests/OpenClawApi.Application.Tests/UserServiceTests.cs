using Moq;
using OpenClawApi.Application.DTOs;
using OpenClawApi.Application.Services;
using OpenClawApi.Domain.Entities;
using OpenClawApi.Domain.Interfaces;
using Xunit;

namespace OpenClawApi.Application.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockRepo;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _mockRepo = new Mock<IUserRepository>();
        _sut = new UserService(_mockRepo.Object);
    }

    [Fact]
    public async Task CreateUserAsync_WithValidData_ReturnsUserResponse()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123"
        };

        _mockRepo.Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);

        _mockRepo.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) =>
            {
                u.Id = 1;
                return u;
            });

        // Act
        var result = await _sut.CreateUserAsync(dto);

        // Assert
        Assert.Equal(1, result.Id);
        Assert.Equal(dto.Username, result.Username);
        Assert.Equal(dto.Email, result.Email);
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Username = "testuser",
            Email = "existing@example.com",
            Password = "Password123"
        };

        _mockRepo.Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync(new User { Id = 1, Email = dto.Email });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.CreateUserAsync(dto));

        Assert.Equal("User with this email already exists", ex.Message);
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithExistingId_ReturnsUser()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow
        };

        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        // Act
        var result = await _sut.GetUserByIdAsync(1);

        // Assert
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Username, result.Username);
        Assert.Equal(user.Email, result.Email);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithNonExistingId_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.GetUserByIdAsync(99));

        Assert.Equal("User not found", ex.Message);
    }

    [Fact]
    public async Task GetUsersAsync_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = 1, Username = "user1", Email = "u1@example.com", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Username = "user2", Email = "u2@example.com", CreatedAt = DateTime.UtcNow }
        };

        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        // Act
        var result = await _sut.GetUsersAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task UpdateUserAsync_WithValidData_ReturnsUpdatedUser()
    {
        // Arrange
        var existingUser = new User
        {
            Id = 1,
            Username = "oldname",
            Email = "old@example.com",
            CreatedAt = DateTime.UtcNow
        };

        var dto = new UpdateUserDto { Username = "newname", Email = "new@example.com" };

        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingUser);
        _mockRepo.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((User?)null);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        // Act
        var result = await _sut.UpdateUserAsync(1, dto);

        // Assert
        Assert.Equal("newname", result.Username);
        Assert.Equal("new@example.com", result.Email);
    }

    [Fact]
    public async Task UpdateUserAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingUser = new User { Id = 1, Username = "user1", Email = "u1@example.com" };
        var otherUser = new User { Id = 2, Username = "user2", Email = "taken@example.com" };
        var dto = new UpdateUserDto { Username = "user1", Email = "taken@example.com" };

        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingUser);
        _mockRepo.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(otherUser);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.UpdateUserAsync(1, dto));
    }

    [Fact]
    public async Task DeleteUserAsync_WithExistingId_ReturnsTrue()
    {
        // Arrange
        var user = new User { Id = 1, Username = "testuser", Email = "test@example.com" };

        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteUserAsync(1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteUserAsync_WithNonExistingId_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.DeleteUserAsync(99));
    }
}
