using Bibliotheque.Api.Models.DTOs;
using Bibliotheque.Api.Models.Entities;
using Bibliotheque.Api.Repositories;

namespace Bibliotheque.Api.Services;

public interface IBookService
{
    Task<IEnumerable<BookDto>> GetAllAsync();
    Task<IEnumerable<BookDto>> GetAllForUserAsync(int accessLevel);
    Task<BookDto?> GetByIdAsync(int id);
    Task<IEnumerable<BookDto>> SearchAsync(string? query);
    Task<IEnumerable<BookDto>> SearchForUserAsync(string? query, int accessLevel);
    Task<BookDto> CreateAsync(CreateBookRequest request);
    Task<BookDto?> UpdateAsync(int id, UpdateBookRequest request);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<BookTypeDto>> GetAllTypesAsync();
}

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly ILoanRepository _loanRepository;

    public BookService(IBookRepository bookRepository, ILoanRepository loanRepository)
    {
        _bookRepository = bookRepository;
        _loanRepository = loanRepository;
    }

    public async Task<IEnumerable<BookDto>> GetAllAsync()
    {
        var books = await _bookRepository.GetAllAsync();
        return books.Select(MapToDto);
    }

    public async Task<IEnumerable<BookDto>> GetAllForUserAsync(int accessLevel)
    {
        var books = await _bookRepository.GetAllForUserAsync(accessLevel);
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

    public async Task<IEnumerable<BookDto>> SearchForUserAsync(string? query, int accessLevel)
    {
        var books = await _bookRepository.SearchForUserAsync(query, accessLevel);
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
            Availability = request.Availability,
            TypeId = request.TypeId
        };

        var created = await _bookRepository.CreateAsync(book);
        // Recharger le livre pour avoir le TypeName
        var reloaded = await _bookRepository.GetByIdAsync(created.Id);
        return MapToDto(reloaded ?? created);
    }

    public async Task<BookDto?> UpdateAsync(int id, UpdateBookRequest request)
    {
        var existing = await _bookRepository.GetByIdAsync(id);
        if (existing == null) return null;

        // Si le livre était emprunté et qu'on le remet disponible, terminer les emprunts actifs
        if (existing.Availability != "available" && request.Availability == "available")
        {
            await _loanRepository.ReturnBookByBookIdAsync(id);
        }

        existing.Title = request.Title;
        existing.Author = request.Author;
        existing.Isbn = request.Isbn;
        existing.Description = request.Description;
        existing.PublicationYear = request.PublicationYear;
        existing.Availability = request.Availability;
        existing.TypeId = request.TypeId;

        var updated = await _bookRepository.UpdateAsync(existing);
        if (!updated) return null;

        // Recharger pour avoir le TypeName mis à jour
        var reloaded = await _bookRepository.GetByIdAsync(id);
        return MapToDto(reloaded ?? existing);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _bookRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<BookTypeDto>> GetAllTypesAsync()
    {
        var types = await _bookRepository.GetAllTypesAsync();
        return types.Select(t => new BookTypeDto
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            MinAccessLevel = t.MinAccessLevel
        });
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
        ImagePath = book.ImagePath,
        TypeId = book.TypeId,
        TypeName = book.TypeName ?? "Étudiants",
        MinAccessLevel = book.MinAccessLevel
    };
}
