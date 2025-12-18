using Xunit;
using Bibliotheque.Api.Models.DTOs;
using Bibliotheque.Api.Models.Entities;

namespace Bibliotheque.Tests.Models;

/// <summary>
/// Tests pour les DTOs et Entities - validation des propriétés
/// </summary>
public class ModelTests
{
    [Fact]
    public void Book_DefaultAvailability_IsAvailable()
    {
        var book = new Book();
        Assert.Equal("available", book.Availability);
    }

    [Fact]
    public void CreateBookRequest_DefaultAvailability_IsAvailable()
    {
        var request = new CreateBookRequest();
        Assert.Equal("available", request.Availability);
    }

    [Fact]
    public void UpdateBookRequest_DefaultAvailability_IsAvailable()
    {
        var request = new UpdateBookRequest();
        Assert.Equal("available", request.Availability);
    }

    [Fact]
    public void RegisterRequest_DefaultRoleId_IsStudent()
    {
        var request = new RegisterRequest();
        Assert.Equal(2, request.RoleId); // 2 = Student
    }

    [Fact]
    public void LoanDto_Properties_CanBeSet()
    {
        var loan = new LoanDto
        {
            Id = 1,
            UserId = 2,
            BookId = 3,
            BookTitle = "Test Book",
            BookAuthor = "Test Author",
            UserName = "John Doe",
            LoanDate = DateTime.Today,
            DueDate = DateTime.Today.AddDays(30),
            ReturnDate = null,
            Status = "active"
        };

        Assert.Equal(1, loan.Id);
        Assert.Equal(2, loan.UserId);
        Assert.Equal(3, loan.BookId);
        Assert.Equal("Test Book", loan.BookTitle);
        Assert.Equal("Test Author", loan.BookAuthor);
        Assert.Equal("John Doe", loan.UserName);
        Assert.Equal("active", loan.Status);
        Assert.Null(loan.ReturnDate);
    }

    [Fact]
    public void BookDto_Properties_CanBeSet()
    {
        var book = new BookDto
        {
            Id = 1,
            Title = "Test Title",
            Author = "Test Author",
            Isbn = "978-1234567890",
            Description = "Test description",
            PublicationYear = 2024,
            Availability = "available",
            ImagePath = "test.jpg"
        };

        Assert.Equal(1, book.Id);
        Assert.Equal("Test Title", book.Title);
        Assert.Equal("Test Author", book.Author);
        Assert.Equal("978-1234567890", book.Isbn);
        Assert.Equal("Test description", book.Description);
        Assert.Equal(2024, book.PublicationYear);
        Assert.Equal("available", book.Availability);
        Assert.Equal("test.jpg", book.ImagePath);
    }

    [Fact]
    public void DashboardStatsDto_Properties_Initialize()
    {
        var stats = new DashboardStatsDto();
        
        Assert.Equal(0, stats.TotalBooks);
        Assert.Equal(0, stats.AvailableBooks);
        Assert.Equal(0, stats.ActiveLoans);
        Assert.Equal(0, stats.OverdueLoans);
        Assert.Equal(0, stats.TotalUsers);
        Assert.NotNull(stats.PopularBooks);
        Assert.Empty(stats.PopularBooks);
    }

    [Fact]
    public void UserDto_Properties_CanBeSet()
    {
        var user = new UserDto
        {
            Id = 1,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Role = "Student"
        };

        Assert.Equal(1, user.Id);
        Assert.Equal("test@example.com", user.Email);
        Assert.Equal("John", user.FirstName);
        Assert.Equal("Doe", user.LastName);
        Assert.Equal("Student", user.Role);
    }

    [Fact]
    public void AuthResponse_Properties_CanBeSet()
    {
        var response = new AuthResponse
        {
            Token = "jwt-token-here",
            User = new UserDto { Id = 1, Email = "test@example.com" }
        };

        Assert.Equal("jwt-token-here", response.Token);
        Assert.NotNull(response.User);
        Assert.Equal("test@example.com", response.User.Email);
    }

    [Fact]
    public void User_Entity_Properties()
    {
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            PasswordHash = "hashed_password",
            FirstName = "John",
            LastName = "Doe",
            RoleId = 2
        };

        Assert.Equal(1, user.Id);
        Assert.Equal("test@example.com", user.Email);
        Assert.Equal("hashed_password", user.PasswordHash);
        Assert.Equal("John", user.FirstName);
        Assert.Equal("Doe", user.LastName);
        Assert.Equal(2, user.RoleId);
    }

    [Fact]
    public void Loan_Entity_DefaultStatus()
    {
        var loan = new Loan();
        Assert.Equal("active", loan.Status);
    }

    [Fact]
    public void PopularBookDto_Properties_CanBeSet()
    {
        var popular = new PopularBookDto
        {
            Title = "Popular Book",
            LoanCount = 42
        };

        Assert.Equal("Popular Book", popular.Title);
        Assert.Equal(42, popular.LoanCount);
    }
}
