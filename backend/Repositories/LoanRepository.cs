using Dapper;
using Bibliotheque.Api.Data;
using Bibliotheque.Api.Models.Entities;

namespace Bibliotheque.Api.Repositories;

public interface ILoanRepository
{
    Task<IEnumerable<Loan>> GetAllAsync();
    Task<IEnumerable<Loan>> GetByUserIdAsync(int userId);
    Task<Loan?> GetByIdAsync(int id);
    Task<Loan?> GetActiveLoanForBookAsync(int bookId);
    Task<Loan> CreateAsync(Loan loan);
    Task<bool> ReturnBookAsync(int loanId);
    Task<bool> ReturnBookByBookIdAsync(int bookId);
    Task<LoanStats> GetStatsAsync();
    Task<IEnumerable<(string Title, int LoanCount)>> GetPopularBooksAsync(int limit = 5);
}

public class LoanStats
{
    public int TotalBooks { get; set; }
    public int AvailableBooks { get; set; }
    public int ActiveLoans { get; set; }
    public int OverdueLoans { get; set; }
    public int TotalUsers { get; set; }
}

public class LoanRepository : ILoanRepository
{
    private readonly DbContext _context;

    public LoanRepository(DbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Loan>> GetAllAsync()
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            SELECT l.id, l.user_id AS UserId, l.book_id AS BookId, 
                   l.loan_date AS LoanDate, l.due_date AS DueDate, 
                   l.return_date AS ReturnDate, l.status, l.created_at AS CreatedAt,
                   b.title AS BookTitle, b.author AS BookAuthor,
                   CONCAT(u.first_name, ' ', u.last_name) AS UserName
            FROM loans l
            JOIN books b ON l.book_id = b.id
            JOIN users u ON l.user_id = u.id
            ORDER BY l.loan_date DESC";
        
        return await connection.QueryAsync<Loan>(sql);
    }

    public async Task<IEnumerable<Loan>> GetByUserIdAsync(int userId)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            SELECT l.id, l.user_id AS UserId, l.book_id AS BookId, 
                   l.loan_date AS LoanDate, l.due_date AS DueDate, 
                   l.return_date AS ReturnDate, l.status, l.created_at AS CreatedAt,
                   b.title AS BookTitle, b.author AS BookAuthor,
                   CONCAT(u.first_name, ' ', u.last_name) AS UserName
            FROM loans l
            JOIN books b ON l.book_id = b.id
            JOIN users u ON l.user_id = u.id
            WHERE l.user_id = @UserId
            ORDER BY l.loan_date DESC";
        
        return await connection.QueryAsync<Loan>(sql, new { UserId = userId });
    }

    public async Task<Loan?> GetByIdAsync(int id)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            SELECT l.id, l.user_id AS UserId, l.book_id AS BookId, 
                   l.loan_date AS LoanDate, l.due_date AS DueDate, 
                   l.return_date AS ReturnDate, l.status, l.created_at AS CreatedAt,
                   b.title AS BookTitle, b.author AS BookAuthor,
                   CONCAT(u.first_name, ' ', u.last_name) AS UserName
            FROM loans l
            JOIN books b ON l.book_id = b.id
            JOIN users u ON l.user_id = u.id
            WHERE l.id = @Id";
        
        return await connection.QueryFirstOrDefaultAsync<Loan>(sql, new { Id = id });
    }

    public async Task<Loan?> GetActiveLoanForBookAsync(int bookId)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            SELECT id, user_id AS UserId, book_id AS BookId, 
                   loan_date AS LoanDate, due_date AS DueDate, 
                   return_date AS ReturnDate, status, created_at AS CreatedAt
            FROM loans
            WHERE book_id = @BookId AND status = 'active'";
        
        return await connection.QueryFirstOrDefaultAsync<Loan>(sql, new { BookId = bookId });
    }

    public async Task<Loan> CreateAsync(Loan loan)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            INSERT INTO loans (user_id, book_id, loan_date, due_date, status)
            VALUES (@UserId, @BookId, @LoanDate, @DueDate, @Status);
            SELECT LAST_INSERT_ID();";
        
        loan.Id = await connection.ExecuteScalarAsync<int>(sql, loan);
        return loan;
    }

    public async Task<bool> ReturnBookAsync(int loanId)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            UPDATE loans 
            SET return_date = CURDATE(), status = 'returned'
            WHERE id = @Id AND status = 'active'";
        
        var affected = await connection.ExecuteAsync(sql, new { Id = loanId });
        return affected > 0;
    }

    public async Task<bool> ReturnBookByBookIdAsync(int bookId)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            UPDATE loans 
            SET return_date = CURDATE(), status = 'returned'
            WHERE book_id = @BookId AND status = 'active'";
        
        var affected = await connection.ExecuteAsync(sql, new { BookId = bookId });
        return affected > 0;
    }

    public async Task<LoanStats> GetStatsAsync()
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            SELECT 
                (SELECT COUNT(*) FROM books) AS TotalBooks,
                (SELECT COUNT(*) FROM books WHERE availability = 'available') AS AvailableBooks,
                (SELECT COUNT(*) FROM loans WHERE status = 'active') AS ActiveLoans,
                (SELECT COUNT(*) FROM loans WHERE status = 'active' AND due_date < CURDATE()) AS OverdueLoans,
                (SELECT COUNT(*) FROM users) AS TotalUsers";
        
        return await connection.QueryFirstAsync<LoanStats>(sql);
    }

    public async Task<IEnumerable<(string Title, int LoanCount)>> GetPopularBooksAsync(int limit = 5)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            SELECT b.title AS Title, COUNT(l.id) AS LoanCount
            FROM books b
            INNER JOIN loans l ON b.id = l.book_id
            GROUP BY b.id, b.title
            ORDER BY COUNT(l.id) DESC
            LIMIT @Limit";
        
        var results = await connection.QueryAsync<(string, int)>(sql, new { Limit = limit });
        return results;
    }
}
