using Moq;
using Xunit;
using Bibliotheque.Api.Models.DTOs;
using Bibliotheque.Api.Models.Entities;
using Bibliotheque.Api.Repositories;
using Bibliotheque.Api.Services;

namespace Bibliotheque.Tests.Services;

public class LoanServiceTests
{
    private readonly Mock<ILoanRepository> _loanRepositoryMock;
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly LoanService _loanService;

    public LoanServiceTests()
    {
        _loanRepositoryMock = new Mock<ILoanRepository>();
        _bookRepositoryMock = new Mock<IBookRepository>();
        _loanService = new LoanService(_loanRepositoryMock.Object, _bookRepositoryMock.Object);
    }

    [Fact]
    public async Task BorrowBookAsync_BookAvailable_ReturnsLoan()
    {
        // Arrange
        var bookId = 1;
        var userId = 1;
        var book = new Book { Id = bookId, Title = "Test Book", Author = "Author", Availability = "available" };
        var createdLoan = new Loan 
        { 
            Id = 1, 
            BookId = bookId, 
            UserId = userId, 
            LoanDate = DateTime.Today, 
            DueDate = DateTime.Today.AddDays(30),
            Status = "active",
            BookTitle = book.Title,
            BookAuthor = book.Author
        };

        _bookRepositoryMock.Setup(x => x.GetByIdAsync(bookId))
            .ReturnsAsync(book);
        _loanRepositoryMock.Setup(x => x.GetActiveLoanForBookAsync(bookId))
            .ReturnsAsync((Loan?)null);
        _loanRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Loan>()))
            .ReturnsAsync(createdLoan);
        _loanRepositoryMock.Setup(x => x.GetByIdAsync(createdLoan.Id))
            .ReturnsAsync(createdLoan);
        _bookRepositoryMock.Setup(x => x.UpdateAvailabilityAsync(bookId, "borrowed"))
            .ReturnsAsync(true);

        // Act
        var result = await _loanService.BorrowBookAsync(userId, bookId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(bookId, result!.BookId);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("active", result.Status);
    }

    [Fact]
    public async Task BorrowBookAsync_BookNotAvailable_ReturnsNull()
    {
        // Arrange
        var bookId = 1;
        var userId = 1;
        var book = new Book { Id = bookId, Title = "Test Book", Author = "Author", Availability = "borrowed" };

        _bookRepositoryMock.Setup(x => x.GetByIdAsync(bookId))
            .ReturnsAsync(book);

        // Act
        var result = await _loanService.BorrowBookAsync(userId, bookId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task BorrowBookAsync_BookNotFound_ReturnsNull()
    {
        // Arrange
        var bookId = 999;
        var userId = 1;

        _bookRepositoryMock.Setup(x => x.GetByIdAsync(bookId))
            .ReturnsAsync((Book?)null);

        // Act
        var result = await _loanService.BorrowBookAsync(userId, bookId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task BorrowBookAsync_BookAlreadyBorrowed_ReturnsNull()
    {
        // Arrange
        var bookId = 1;
        var userId = 1;
        var book = new Book { Id = bookId, Title = "Test Book", Author = "Author", Availability = "available" };
        var existingLoan = new Loan { Id = 1, BookId = bookId, UserId = 2, Status = "active" };

        _bookRepositoryMock.Setup(x => x.GetByIdAsync(bookId))
            .ReturnsAsync(book);
        _loanRepositoryMock.Setup(x => x.GetActiveLoanForBookAsync(bookId))
            .ReturnsAsync(existingLoan);

        // Act
        var result = await _loanService.BorrowBookAsync(userId, bookId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ReturnBookAsync_ValidLoan_ReturnsTrue()
    {
        // Arrange
        var loanId = 1;
        var userId = 1;
        var loan = new Loan 
        { 
            Id = loanId, 
            BookId = 1, 
            UserId = userId, 
            Status = "active",
            BookTitle = "Test Book",
            BookAuthor = "Author"
        };

        _loanRepositoryMock.Setup(x => x.GetByIdAsync(loanId))
            .ReturnsAsync(loan);
        _loanRepositoryMock.Setup(x => x.ReturnBookAsync(loanId))
            .ReturnsAsync(true);
        _bookRepositoryMock.Setup(x => x.UpdateAvailabilityAsync(loan.BookId, "available"))
            .ReturnsAsync(true);

        // Act
        var result = await _loanService.ReturnBookAsync(loanId, userId);

        // Assert
        Assert.True(result);
        _bookRepositoryMock.Verify(x => x.UpdateAvailabilityAsync(loan.BookId, "available"), Times.Once);
    }

    [Fact]
    public async Task ReturnBookAsync_LoanNotFound_ReturnsFalse()
    {
        // Arrange
        var loanId = 999;
        var userId = 1;

        _loanRepositoryMock.Setup(x => x.GetByIdAsync(loanId))
            .ReturnsAsync((Loan?)null);

        // Act
        var result = await _loanService.ReturnBookAsync(loanId, userId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ReturnBookAsync_WrongUser_ReturnsFalse()
    {
        // Arrange
        var loanId = 1;
        var userId = 1;
        var otherUserId = 2;
        var loan = new Loan 
        { 
            Id = loanId, 
            BookId = 1, 
            UserId = otherUserId, 
            Status = "active" 
        };

        _loanRepositoryMock.Setup(x => x.GetByIdAsync(loanId))
            .ReturnsAsync(loan);

        // Act
        var result = await _loanService.ReturnBookAsync(loanId, userId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ReturnBookAsync_AlreadyReturned_ReturnsFalse()
    {
        // Arrange
        var loanId = 1;
        var userId = 1;
        var loan = new Loan 
        { 
            Id = loanId, 
            BookId = 1, 
            UserId = userId, 
            Status = "returned" 
        };

        _loanRepositoryMock.Setup(x => x.GetByIdAsync(loanId))
            .ReturnsAsync(loan);

        // Act
        var result = await _loanService.ReturnBookAsync(loanId, userId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_ReturnsCorrectStats()
    {
        // Arrange
        var stats = new LoanStats
        {
            TotalBooks = 100,
            AvailableBooks = 80,
            ActiveLoans = 20,
            OverdueLoans = 3,
            TotalUsers = 50
        };

        var popularBooks = new List<(string Title, int LoanCount)>
        {
            ("Book 1", 25),
            ("Book 2", 18),
            ("Book 3", 15)
        };

        _loanRepositoryMock.Setup(x => x.GetStatsAsync())
            .ReturnsAsync(stats);
        _loanRepositoryMock.Setup(x => x.GetPopularBooksAsync(5))
            .ReturnsAsync(popularBooks);

        // Act
        var result = await _loanService.GetDashboardStatsAsync();

        // Assert
        Assert.Equal(100, result.TotalBooks);
        Assert.Equal(80, result.AvailableBooks);
        Assert.Equal(20, result.ActiveLoans);
        Assert.Equal(3, result.OverdueLoans);
        Assert.Equal(50, result.TotalUsers);
        Assert.Equal(3, result.PopularBooks.Count);
        Assert.Equal("Book 1", result.PopularBooks[0].Title);
        Assert.Equal(25, result.PopularBooks[0].LoanCount);
    }
}
