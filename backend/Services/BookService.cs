using Bibliotheque.Api.Models.DTOs;
using Bibliotheque.Api.Models.Entities;
using Bibliotheque.Api.Repositories;

namespace Bibliotheque.Api.Services;

public interface IBookService
{
    Task<IEnumerable<BookDto>> GetAllAsync();
    Task<BookDto?> GetByIdAsync(int id);
    Task<IEnumerable<BookDto>> SearchAsync(string? query);
    Task<BookDto> CreateAsync(CreateBookRequest request);
    Task<BookDto?> UpdateAsync(int id, UpdateBookRequest request);
    Task<bool> DeleteAsync(int id);
}

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;

    public BookService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<IEnumerable<BookDto>> GetAllAsync()
    {
        var books = await _bookRepository.GetAllAsync();
        return books.Select(MapToDto);
    }

    public async Task<BookDto?> GetByIdAsync(int id)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        return book == null ? null : MapToDto(book);
    }

    public async Task<IEnumerable<BookDto>> SearchAsync(string? query)
    {
        var books = await _bookRepository.SearchAsync(query);
        return books.Select(MapToDto);
    }

    public async Task<BookDto> CreateAsync(CreateBookRequest request)
    {
        var book = new Book
        {
            Title = request.Title,
            Author = request.Author,
            Isbn = request.Isbn,
            Description = request.Description,
            PublicationYear = request.PublicationYear,
            Availability = request.Availability
        };

        var created = await _bookRepository.CreateAsync(book);
        return MapToDto(created);
    }

    public async Task<BookDto?> UpdateAsync(int id, UpdateBookRequest request)
    {
        var existing = await _bookRepository.GetByIdAsync(id);
        if (existing == null) return null;

        existing.Title = request.Title;
        existing.Author = request.Author;
        existing.Isbn = request.Isbn;
        existing.Description = request.Description;
        existing.PublicationYear = request.PublicationYear;
        existing.Availability = request.Availability;

        var updated = await _bookRepository.UpdateAsync(existing);
        if (!updated) return null;

        return MapToDto(existing);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _bookRepository.DeleteAsync(id);
    }

    private static BookDto MapToDto(Book book) => new()
    {
        Id = book.Id,
        Title = book.Title,
        Author = book.Author,
        Isbn = book.Isbn,
        Description = book.Description,
        PublicationYear = book.PublicationYear,
        Availability = book.Availability,
        ImagePath = book.ImagePath
    };
}
