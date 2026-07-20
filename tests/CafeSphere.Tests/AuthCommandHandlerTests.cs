using CafeSphere.Application.Features.Auth;
using CafeSphere.Application.Interfaces;
using CafeSphere.Domain.Entities;
using CafeSphere.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace CafeSphere.Tests;

public class AuthCommandHandlerTests
{
    private readonly Mock<IMongoRepository<User>> _userRepoMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtService> _jwtServiceMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();

    [Fact]
    public async Task RegisterUser_Should_Return_Success_When_Valid()
    {
        // Arrange
        _userRepoMock.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _passwordHasherMock.Setup(p => p.HashPassword(It.IsAny<string>()))
            .Returns("hashed_password_sample");

        _jwtServiceMock.Setup(j => j.GenerateRefreshToken(It.IsAny<string>()))
            .Returns(new RefreshToken { Token = "sample_refresh_token", ExpiresAt = DateTime.UtcNow.AddDays(7) });

        _jwtServiceMock.Setup(j => j.GenerateAccessToken(It.IsAny<User>(), null))
            .Returns("sample_jwt_access_token");

        _currentUserServiceMock.Setup(c => c.IpAddress).Returns("127.0.0.1");

        var handler = new RegisterUserCommandHandler(
            _userRepoMock.Object,
            _passwordHasherMock.Object,
            _jwtServiceMock.Object,
            _emailServiceMock.Object,
            _currentUserServiceMock.Object
        );

        var command = new RegisterUserCommand("john_doe", "john@example.com", "Password123!", "John Doe", "+1234567890");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Email.Should().Be("john@example.com");
        result.Value.AccessToken.Should().Be("sample_jwt_access_token");

        _userRepoMock.Verify(r => r.InsertAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
