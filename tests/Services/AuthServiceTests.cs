using Moq;
using Xunit;
using Microsoft.Extensions.Configuration;
using Bibliotheque.Api.Models.DTOs;
using Bibliotheque.Api.Models.Entities;
using Bibliotheque.Api.Repositories;
using Bibliotheque.Api.Services;

namespace Bibliotheque.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _configurationMock = new Mock<IConfiguration>();

        // Setup JWT configuration
        _configurationMock.Setup(x => x["Jwt:Key"]).Returns("SuperSecretKeyForTestingPurposesOnly123!");
        _configurationMock.Setup(x => x["Jwt:Issuer"]).Returns("TestIssuer");
        _configurationMock.Setup(x => x["Jwt:Audience"]).Returns("TestAudience");
        _configurationMock.Setup(x => x["Jwt:ExpireMinutes"]).Returns("60");

        _authService = new AuthService(_userRepositoryMock.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123");
        var user = new User
        {
            Id = 1,
            Email = "test@test.com",
            PasswordHash = passwordHash,
            FirstName = "Test",
            LastName = "User",
            RoleName = "Student"
        };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync("test@test.com"))
            .ReturnsAsync(user);

        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = "password123"
        };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result!.Token);
        Assert.Equal("test@test.com", result.User.Email);
        Assert.Equal("Test", result.User.FirstName);
    }

    [Fact]
    public async Task LoginAsync_InvalidEmail_ReturnsNull()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetByEmailAsync("nonexistent@test.com"))
            .ReturnsAsync((User?)null);

        var request = new LoginRequest
        {
            Email = "nonexistent@test.com",
            Password = "password123"
        };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsNull()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword");
        var user = new User
        {
            Id = 1,
            Email = "test@test.com",
            PasswordHash = passwordHash,
            FirstName = "Test",
            LastName = "User"
        };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync("test@test.com"))
            .ReturnsAsync(user);

        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = "wrongpassword"
        };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RegisterAsync_NewEmail_CreatesUserAndReturnsAuthResponse()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "newuser@test.com",
            Password = "password123",
            FirstName = "New",
            LastName = "User",
            RoleId = 2
        };

        var createdUser = new User
        {
            Id = 1,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            RoleId = request.RoleId
        };

        var fullUser = new User
        {
            Id = 1,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            RoleId = request.RoleId,
            RoleName = "Student"
        };

        _userRepositoryMock.Setup(x => x.EmailExistsAsync(request.Email))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(createdUser.Id))
            .ReturnsAsync(fullUser);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result!.Token);
        Assert.Equal("newuser@test.com", result.User.Email);
        Assert.Equal("New", result.User.FirstName);
    }

    [Fact]
    public async Task RegisterAsync_ExistingEmail_ReturnsNull()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@test.com",
            Password = "password123",
            FirstName = "Existing",
            LastName = "User",
            RoleId = 2
        };

        _userRepositoryMock.Setup(x => x.EmailExistsAsync(request.Email))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GenerateToken_ValidUser_ReturnsNonEmptyToken()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@test.com",
            FirstName = "Test",
            LastName = "User",
            RoleName = "Librarian"
        };

        // Act
        var token = _authService.GenerateToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.Contains(".", token); // JWT tokens contain dots
    }

    [Fact]
    public void GenerateToken_IncludesCorrectClaims()
    {
        // Arrange
        var user = new User
        {
            Id = 42,
            Email = "claims@test.com",
            FirstName = "Claims",
            LastName = "Test",
            RoleName = "Librarian",
            AccessLevel = 3
        };

        // Act
        var token = _authService.GenerateToken(user);

        // Assert
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        // Vérifier les claims essentiels
        Assert.Contains(jwtToken.Claims, c => c.Value == "42");
        Assert.Contains(jwtToken.Claims, c => c.Value == "claims@test.com");
        Assert.Contains(jwtToken.Claims, c => c.Value == "Librarian");
        Assert.Contains(jwtToken.Claims, c => c.Type == "AccessLevel" && c.Value == "3");
    }
}
