using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using UserDocumentAPI.BackgroundJobs;
using UserDocumentAPI.Data;
using UserDocumentAPI.Models;
using Xunit;
using FluentAssertions;

namespace UserDocumentAPI.Tests.BackgroundJobs;

public class WelcomeMessageJobTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly WelcomeMessageJob _sut;
    private readonly Mock<ILogger<WelcomeMessageJob>> _loggerMock;
    private readonly ServiceProvider _serviceProvider;

    public WelcomeMessageJobTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _loggerMock = new Mock<ILogger<WelcomeMessageJob>>();

        // Setup ServiceProvider with DbContext
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(opt => 
            opt.UseInMemoryDatabase(databaseName: _context.Database.GetDbConnection().Database));
        services.AddScoped(_ => _context);
        _serviceProvider = services.BuildServiceProvider();

        _sut = new WelcomeMessageJob(_loggerMock.Object, _serviceProvider);
    }

    [Fact]
    public async Task SendWelcomeMessageAsync_ValidUserId_CompletesSuccessfully()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@example.com",
            RegisteredAt = DateTime.UtcNow
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var action = async () => await _sut.SendWelcomeMessageAsync(user.Id);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendWelcomeMessageAsync_InvalidUserId_LogsWarning()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        await _sut.SendWelcomeMessageAsync(invalidId);

        // Assert - verify warning was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendWelcomeMessageAsync_ValidUser_LogsInformationMessages()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Email = "john@example.com",
            RegisteredAt = DateTime.UtcNow
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        await _sut.SendWelcomeMessageAsync(user.Id);

        // Assert - verify information logs were called
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeast(2)); // Sending + Sent messages
    }

    public void Dispose()
    {
        _context.Dispose();
        _serviceProvider.Dispose();
    }
}
