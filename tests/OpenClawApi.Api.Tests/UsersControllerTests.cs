using Microsoft.AspNetCore.Mvc;
using Moq;
using OpenClawApi.Api.Controllers;
using OpenClawApi.Application.DTOs;
using OpenClawApi.Application.Interfaces;
using Xunit;

namespace OpenClawApi.Api.Tests;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _mockService;
    private readonly UsersController _sut;

    public UsersControllerTests()
    {
        _mockService = new Mock<IUserService>();
        _sut = new UsersController(_mockService.Object);
    }

    [Fact]
    public async Task CreateUser_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123"
        };

        var response = new UserResponseDto
        {
            Id = 1,
            Username = dto.Username,
            Email = dto.Email,
            CreatedAt = DateTime.UtcNow
        };

        _mockService.Setup(s => s.CreateUserAsync(dto)).ReturnsAsync(response);

        // Act
        var result = await _sut.CreateUser(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnValue = Assert.IsType<UserResponseDto>(createdResult.Value);
        Assert.Equal(dto.Email, returnValue.Email);
    }

    [Fact]
    public async Task CreateUser_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Username = "testuser",
            Email = "existing@example.com",
            Password = "Password123"
        };

        _mockService.Setup(s => s.CreateUserAsync(dto))
            .ThrowsAsync(new InvalidOperationException("User with this email already exists"));

        // Act
        var result = await _sut.CreateUser(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetUsers_ReturnsOkWithUsers()
    {
        // Arrange
        var users = new List<UserResponseDto>
        {
            new() { Id = 1, Username = "user1", Email = "u1@example.com" },
            new() { Id = 2, Username = "user2", Email = "u2@example.com" }
        };

        _mockService.Setup(s => s.GetUsersAsync()).ReturnsAsync(users);

        // Act
        var result = await _sut.GetUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsAssignableFrom<IReadOnlyList<UserResponseDto>>(okResult.Value);
        Assert.Equal(2, returnValue.Count);
    }

    [Fact]
    public async Task GetUser_WithExistingId_ReturnsOk()
    {
        // Arrange
        var user = new UserResponseDto { Id = 1, Username = "testuser", Email = "test@example.com" };
        _mockService.Setup(s => s.GetUserByIdAsync(1)).ReturnsAsync(user);

        // Act
        var result = await _sut.GetUser(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<UserResponseDto>(okResult.Value);
        Assert.Equal(1, returnValue.Id);
    }

    [Fact]
    public async Task GetUser_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.GetUserByIdAsync(99))
            .ThrowsAsync(new InvalidOperationException("User not found"));

        // Act
        var result = await _sut.GetUser(99);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateUser_WithValidData_ReturnsOk()
    {
        // Arrange
        var dto = new UpdateUserDto { Username = "updated", Email = "updated@example.com" };
        var response = new UserResponseDto
        {
            Id = 1,
            Username = dto.Username,
            Email = dto.Email,
            CreatedAt = DateTime.UtcNow
        };

        _mockService.Setup(s => s.UpdateUserAsync(1, dto)).ReturnsAsync(response);

        // Act
        var result = await _sut.UpdateUser(1, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<UserResponseDto>(okResult.Value);
        Assert.Equal("updated", returnValue.Username);
    }

    [Fact]
    public async Task DeleteUser_WithExistingId_ReturnsNoContent()
    {
        // Arrange
        _mockService.Setup(s => s.DeleteUserAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteUser(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteUser_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.DeleteUserAsync(99))
            .ThrowsAsync(new InvalidOperationException("User not found"));

        // Act
        var result = await _sut.DeleteUser(99);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
