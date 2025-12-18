using Dapper;
using Bibliotheque.Api.Data;
using Bibliotheque.Api.Models.Entities;

namespace Bibliotheque.Api.Repositories;

public interface IBookRepository
{
    Task<IEnumerable<Book>> GetAllAsync();
    Task<IEnumerable<Book>> GetAllForUserAsync(int accessLevel);
    Task<Book?> GetByIdAsync(int id);
    Task<IEnumerable<Book>> SearchAsync(string? query);
    Task<IEnumerable<Book>> SearchForUserAsync(string? query, int accessLevel);
    Task<Book> CreateAsync(Book book);
    Task<bool> UpdateAsync(Book book);
    Task<bool> DeleteAsync(int id);
    Task<bool> UpdateAvailabilityAsync(int id, string availability);
    Task<IEnumerable<BookType>> GetAllTypesAsync();
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
            SELECT b.id, b.title, b.author, b.isbn, b.description, 
                   b.publication_year AS PublicationYear, b.availability, 
                   b.image_path AS ImagePath, b.type_id AS TypeId,
                   bt.name AS TypeName, bt.min_access_level AS MinAccessLevel,
                   b.created_at AS CreatedAt, b.updated_at AS UpdatedAt
            FROM books b
            LEFT JOIN book_types bt ON b.type_id = bt.id
            ORDER BY b.title";
        
        return await connection.QueryAsync<Book>(sql);
    }

    public async Task<IEnumerable<Book>> GetAllForUserAsync(int accessLevel)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            SELECT b.id, b.title, b.author, b.isbn, b.description, 
                   b.publication_year AS PublicationYear, b.availability, 
                   b.image_path AS ImagePath, b.type_id AS TypeId,
                   bt.name AS TypeName, bt.min_access_level AS MinAccessLevel,
                   b.created_at AS CreatedAt, b.updated_at AS UpdatedAt
            FROM books b
            LEFT JOIN book_types bt ON b.type_id = bt.id
            WHERE bt.min_access_level <= @AccessLevel
            ORDER BY b.title";
        
        return await connection.QueryAsync<Book>(sql, new { AccessLevel = accessLevel });
    }

    public async Task<Book?> GetByIdAsync(int id)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            SELECT b.id, b.title, b.author, b.isbn, b.description, 
                   b.publication_year AS PublicationYear, b.availability, 
                   b.image_path AS ImagePath, b.type_id AS TypeId,
                   bt.name AS TypeName, bt.min_access_level AS MinAccessLevel,
                   b.created_at AS CreatedAt, b.updated_at AS UpdatedAt
            FROM books b
            LEFT JOIN book_types bt ON b.type_id = bt.id
            WHERE b.id = @Id";
        
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
            SELECT b.id, b.title, b.author, b.isbn, b.description, 
                   b.publication_year AS PublicationYear, b.availability, 
                   b.image_path AS ImagePath, b.type_id AS TypeId,
                   bt.name AS TypeName, bt.min_access_level AS MinAccessLevel,
                   b.created_at AS CreatedAt, b.updated_at AS UpdatedAt
            FROM books b
            LEFT JOIN book_types bt ON b.type_id = bt.id
            WHERE b.title LIKE @Query 
               OR b.author LIKE @Query 
               OR b.isbn LIKE @Query
            ORDER BY b.title";
        
        return await connection.QueryAsync<Book>(sql, new { Query = $"%{query}%" });
    }

    public async Task<IEnumerable<Book>> SearchForUserAsync(string? query, int accessLevel)
    {
        using var connection = _context.CreateConnection();
        
        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetAllForUserAsync(accessLevel);
        }

        const string sql = @"
            SELECT b.id, b.title, b.author, b.isbn, b.description, 
                   b.publication_year AS PublicationYear, b.availability, 
                   b.image_path AS ImagePath, b.type_id AS TypeId,
                   bt.name AS TypeName, bt.min_access_level AS MinAccessLevel,
                   b.created_at AS CreatedAt, b.updated_at AS UpdatedAt
            FROM books b
            LEFT JOIN book_types bt ON b.type_id = bt.id
            WHERE bt.min_access_level <= @AccessLevel
              AND (b.title LIKE @Query OR b.author LIKE @Query OR b.isbn LIKE @Query)
            ORDER BY b.title";
        
        return await connection.QueryAsync<Book>(sql, new { Query = $"%{query}%", AccessLevel = accessLevel });
    }

    public async Task<Book> CreateAsync(Book book)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            INSERT INTO books (title, author, isbn, description, publication_year, availability, image_path, type_id)
            VALUES (@Title, @Author, @Isbn, @Description, @PublicationYear, @Availability, @ImagePath, @TypeId);
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
                image_path = @ImagePath,
                type_id = @TypeId
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

    public async Task<IEnumerable<BookType>> GetAllTypesAsync()
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            SELECT id, name, description, min_access_level AS MinAccessLevel
            FROM book_types
            ORDER BY min_access_level, name";
        
        return await connection.QueryAsync<BookType>(sql);
    }
}
