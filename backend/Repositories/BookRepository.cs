using Dapper;
using Bibliotheque.Api.Data;
using Bibliotheque.Api.Models.Entities;

namespace Bibliotheque.Api.Repositories;

public interface IBookRepository
{
    Task<IEnumerable<Book>> GetAllAsync();
    Task<Book?> GetByIdAsync(int id);
    Task<IEnumerable<Book>> SearchAsync(string? query);
    Task<Book> CreateAsync(Book book);
    Task<bool> UpdateAsync(Book book);
    Task<bool> DeleteAsync(int id);
    Task<bool> UpdateAvailabilityAsync(int id, string availability);
}

public class BookRepository : IBookRepository
{
    private readonly DbContext _context;

    public BookRepository(DbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Book>> GetAllAsync()
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            SELECT id, title, author, isbn, description, 
                   publication_year AS PublicationYear, availability, 
                   image_path AS ImagePath, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM books
            ORDER BY title";
        
        return await connection.QueryAsync<Book>(sql);
    }

    public async Task<Book?> GetByIdAsync(int id)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            SELECT id, title, author, isbn, description, 
                   publication_year AS PublicationYear, availability, 
                   image_path AS ImagePath, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM books
            WHERE id = @Id";
        
        return await connection.QueryFirstOrDefaultAsync<Book>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Book>> SearchAsync(string? query)
    {
        using var connection = _context.CreateConnection();
        
        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetAllAsync();
        }

        const string sql = @"
            SELECT id, title, author, isbn, description, 
                   publication_year AS PublicationYear, availability, 
                   image_path AS ImagePath, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM books
            WHERE title LIKE @Query 
               OR author LIKE @Query 
               OR isbn LIKE @Query
            ORDER BY title";
        
        return await connection.QueryAsync<Book>(sql, new { Query = $"%{query}%" });
    }

    public async Task<Book> CreateAsync(Book book)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            INSERT INTO books (title, author, isbn, description, publication_year, availability, image_path)
            VALUES (@Title, @Author, @Isbn, @Description, @PublicationYear, @Availability, @ImagePath);
            SELECT LAST_INSERT_ID();";
        
        book.Id = await connection.ExecuteScalarAsync<int>(sql, book);
        return book;
    }

    public async Task<bool> UpdateAsync(Book book)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            UPDATE books 
            SET title = @Title, 
                author = @Author, 
                isbn = @Isbn, 
                description = @Description, 
                publication_year = @PublicationYear, 
                availability = @Availability,
                image_path = @ImagePath
            WHERE id = @Id";
        
        var affected = await connection.ExecuteAsync(sql, book);
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _context.CreateConnection();
        const string sql = "DELETE FROM books WHERE id = @Id";
        var affected = await connection.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }

    public async Task<bool> UpdateAvailabilityAsync(int id, string availability)
    {
        using var connection = _context.CreateConnection();
        const string sql = "UPDATE books SET availability = @Availability WHERE id = @Id";
        var affected = await connection.ExecuteAsync(sql, new { Id = id, Availability = availability });
        return affected > 0;
    }
}
