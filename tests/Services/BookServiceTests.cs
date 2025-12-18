using Moq;
using Xunit;
using Bibliotheque.Api.Models.DTOs;
using Bibliotheque.Api.Models.Entities;
using Bibliotheque.Api.Repositories;
using Bibliotheque.Api.Services;

namespace Bibliotheque.Tests.Services;

public class BookServiceTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<ILoanRepository> _loanRepositoryMock;
    private readonly BookService _bookService;

    public BookServiceTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _loanRepositoryMock = new Mock<ILoanRepository>();
        _bookService = new BookService(_bookRepositoryMock.Object, _loanRepositoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllBooks()
    {
        // Arrange
        var books = new List<Book>
        {
            new Book { Id = 1, Title = "Book 1", Author = "Author 1", Availability = "available" },
            new Book { Id = 2, Title = "Book 2", Author = "Author 2", Availability = "borrowed" }
        };

        _bookRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(books);

        // Act
        var result = await _bookService.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_BookExists_ReturnsBook()
    {
        // Arrange
        var book = new Book 
        { 
            Id = 1, 
            Title = "Test Book", 
            Author = "Test Author", 
            Isbn = "978-1234567890",
            Description = "A test book",
            PublicationYear = 2024,
            Availability = "available" 
        };

        _bookRepositoryMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(book);

        // Act
        var result = await _bookService.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Book", result!.Title);
        Assert.Equal("Test Author", result.Author);
        Assert.Equal("978-1234567890", result.Isbn);
    }

    [Fact]
    public async Task GetByIdAsync_BookNotExists_ReturnsNull()
    {
        // Arrange
        _bookRepositoryMock.Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Book?)null);

        // Act
        var result = await _bookService.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesBook()
    {
        // Arrange
        var request = new CreateBookRequest
        {
            Title = "New Book",
            Author = "New Author",
            Isbn = "978-0987654321",
            Description = "A new book",
            PublicationYear = 2024,
            Availability = "available"
        };

        var createdBook = new Book
        {
            Id = 1,
            Title = request.Title,
            Author = request.Author,
            Isbn = request.Isbn,
            Description = request.Description,
            PublicationYear = request.PublicationYear,
            Availability = request.Availability
        };

        _bookRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Book>()))
            .ReturnsAsync(createdBook);

        // Act
        var result = await _bookService.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Book", result.Title);
        Assert.Equal("New Author", result.Author);
    }

    [Fact]
    public async Task SearchAsync_ReturnsMatchingBooks()
    {
        // Arrange
        var books = new List<Book>
        {
            new Book { Id = 1, Title = "C# Programming", Author = "Author 1", Availability = "available" },
            new Book { Id = 2, Title = "Java Programming", Author = "Author 2", Availability = "available" }
        };

        _bookRepositoryMock.Setup(x => x.SearchAsync("Programming"))
            .ReturnsAsync(books);

        // Act
        var result = await _bookService.SearchAsync("Programming");

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, b => Assert.Contains("Programming", b.Title));
    }

    [Fact]
    public async Task UpdateAsync_BookExists_UpdatesBook()
    {
        // Arrange
        var existingBook = new Book
        {
            Id = 1,
            Title = "Old Title",
            Author = "Old Author",
            Availability = "available"
        };

        var updateRequest = new UpdateBookRequest
        {
            Title = "Updated Title",
            Author = "Updated Author",
            Isbn = "978-1111111111",
            Description = "Updated description",
            PublicationYear = 2025,
            Availability = "available"
        };

        _bookRepositoryMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(existingBook);
        _bookRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Book>()))
            .ReturnsAsync(true);

        // Act
        var result = await _bookService.UpdateAsync(1, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Title", result!.Title);
        Assert.Equal("Updated Author", result.Author);
    }

    [Fact]
    public async Task UpdateAsync_BookNotExists_ReturnsNull()
    {
        // Arrange
        var updateRequest = new UpdateBookRequest
        {
            Title = "Updated Title",
            Author = "Updated Author",
            Availability = "available"
        };

        _bookRepositoryMock.Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Book?)null);

        // Act
        var result = await _bookService.UpdateAsync(999, updateRequest);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_BookExists_ReturnsTrue()
    {
        // Arrange
        _bookRepositoryMock.Setup(x => x.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _bookService.DeleteAsync(1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_BookNotExists_ReturnsFalse()
    {
        // Arrange
        _bookRepositoryMock.Setup(x => x.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _bookService.DeleteAsync(999);

        // Assert
        Assert.False(result);
    }
}
