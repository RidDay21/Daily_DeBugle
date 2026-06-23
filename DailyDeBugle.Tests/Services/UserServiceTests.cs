using DailyDeBugle.Data;
using DailyDeBugle.Models;
using DailyDeBugle.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DailyDeBugle.Tests.Services;

public class UserServiceTests
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new ApplicationDbContext(options);
        dbContext.Database.EnsureCreated();
        return dbContext;
    }

    [Fact]
    public async Task RegisterAsync_ShouldHashPasswordAndSaveUser()
    {
        // Arrange
        var dbContext = GetDbContext();
        var userService = new UserService(dbContext);
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            Role = UserRole.Author
        };
        var password = "SecurePassword123";

        // Act
        var result = await userService.RegisterAsync(user, password);

        // Assert
        Assert.NotNull(result.PasswordHash);
        Assert.NotEqual(password, result.PasswordHash);
        var savedUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
        Assert.NotNull(savedUser);
        Assert.Equal(UserRole.Author, savedUser.Role);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnUser_WhenCredentialsAreValid()
    {
        // Arrange
        var dbContext = GetDbContext();
        var userService = new UserService(dbContext);
        var password = "SecurePassword123";
        var user = new User
        {
            Username = "authuser",
            Email = "auth@example.com",
            Role = UserRole.Reader
        };
        await userService.RegisterAsync(user, password);

        // Act
        var authenticatedUser = await userService.AuthenticateAsync("authuser", password);

        // Assert
        Assert.NotNull(authenticatedUser);
        Assert.Equal("authuser", authenticatedUser.Username);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnNull_WhenPasswordIsInvalid()
    {
        // Arrange
        var dbContext = GetDbContext();
        var userService = new UserService(dbContext);
        var password = "SecurePassword123";
        var user = new User
        {
            Username = "wrongpassuser",
            Email = "wrong@example.com",
            Role = UserRole.Reader
        };
        await userService.RegisterAsync(user, password);

        // Act
        var authenticatedUser = await userService.AuthenticateAsync("wrongpassuser", "WrongPassword");

        // Assert
        Assert.Null(authenticatedUser);
    }

    [Fact]
    public async Task UpdateUserRoleAsync_ShouldChangeUserRole()
    {
        // Arrange
        var dbContext = GetDbContext();
        var userService = new UserService(dbContext);
        var user = new User
        {
            Username = "roleuser",
            Email = "role@example.com",
            Role = UserRole.Reader
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Act
        await userService.UpdateUserRoleAsync(user.UserId, UserRole.Admin);

        // Assert
        var updatedUser = await dbContext.Users.FindAsync(user.UserId);
        Assert.Equal(UserRole.Admin, updatedUser.Role);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnUsersSortedByUsername()
    {
        // Arrange
        var dbContext = GetDbContext();
        var userService = new UserService(dbContext);
        dbContext.Users.AddRange(new List<User>
        {
            new User { Username = "Zebra", Email = "z@ex.com" },
            new User { Username = "Apple", Email = "a@ex.com" },
            new User { Username = "Monkey", Email = "m@ex.com" }
        });
        await dbContext.SaveChangesAsync();

        // Act
        var users = await userService.GetAllAsync();

        // Assert
        Assert.Equal(3, users.Count);
        Assert.Equal("Apple", users[0].Username);
        Assert.Equal("Monkey", users[1].Username);
        Assert.Equal("Zebra", users[2].Username);
    }
}
