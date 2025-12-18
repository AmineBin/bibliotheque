using Bibliotheque.Api.Models.DTOs;
using Bibliotheque.Api.Models.Entities;
using Bibliotheque.Api.Repositories;

namespace Bibliotheque.Api.Services;

public interface ILoanService
{
    Task<IEnumerable<LoanDto>> GetAllAsync();
    Task<IEnumerable<LoanDto>> GetByUserIdAsync(int userId);
    Task<LoanDto?> GetByIdAsync(int id);
    Task<LoanDto?> BorrowBookAsync(int userId, int bookId);
    Task<bool> ReturnBookAsync(int loanId, int userId);
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}

public class LoanService : ILoanService
{
    private readonly ILoanRepository _loanRepository;
    private readonly IBookRepository _bookRepository;

    public LoanService(ILoanRepository loanRepository, IBookRepository bookRepository)
    {
        _loanRepository = loanRepository;
        _bookRepository = bookRepository;
    }

    public async Task<IEnumerable<LoanDto>> GetAllAsync()
    {
        var loans = await _loanRepository.GetAllAsync();
        return loans.Select(MapToDto);
    }

    public async Task<IEnumerable<LoanDto>> GetByUserIdAsync(int userId)
    {
        var loans = await _loanRepository.GetByUserIdAsync(userId);
        return loans.Select(MapToDto);
    }

    public async Task<LoanDto?> GetByIdAsync(int id)
    {
        var loan = await _loanRepository.GetByIdAsync(id);
        return loan == null ? null : MapToDto(loan);
    }

    public async Task<LoanDto?> BorrowBookAsync(int userId, int bookId)
    {
        // Verifier que le livre existe et est disponible
        var book = await _bookRepository.GetByIdAsync(bookId);
        if (book == null || book.Availability != "available")
        {
            return null;
        }

        // Verifier qu'il n'y a pas deja un emprunt actif pour ce livre
        var existingLoan = await _loanRepository.GetActiveLoanForBookAsync(bookId);
        if (existingLoan != null)
        {
            return null;
        }

        // Creer l'emprunt (30 jours par defaut)
        var loan = new Loan
        {
            UserId = userId,
            BookId = bookId,
            LoanDate = DateTime.Today,
            DueDate = DateTime.Today.AddDays(30),
            Status = "active"
        };

        var created = await _loanRepository.CreateAsync(loan);
        
        // Mettre a jour la disponibilite du livre
        await _bookRepository.UpdateAvailabilityAsync(bookId, "borrowed");

        // Recuperer le loan complet avec les infos du livre
        return await GetByIdAsync(created.Id);
    }

    public async Task<bool> ReturnBookAsync(int loanId, int userId)
    {
        var loan = await _loanRepository.GetByIdAsync(loanId);
        if (loan == null || loan.UserId != userId || loan.Status != "active")
        {
            return false;
        }

        var returned = await _loanRepository.ReturnBookAsync(loanId);
        if (returned)
        {
            // Remettre le livre disponible
            await _bookRepository.UpdateAvailabilityAsync(loan.BookId, "available");
        }

        return returned;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var stats = await _loanRepository.GetStatsAsync();
        var popularBooks = await _loanRepository.GetPopularBooksAsync(5);
        
        return new DashboardStatsDto
        {
            TotalBooks = stats.TotalBooks,
            AvailableBooks = stats.AvailableBooks,
            ActiveLoans = stats.ActiveLoans,
            OverdueLoans = stats.OverdueLoans,
            TotalUsers = stats.TotalUsers,
            PopularBooks = popularBooks.Select(p => new PopularBookDto
            {
                Title = p.Title,
                LoanCount = p.LoanCount
            }).ToList()
        };
    }

    private static LoanDto MapToDto(Loan loan) => new()
    {
        Id = loan.Id,
        UserId = loan.UserId,
        BookId = loan.BookId,
        BookTitle = loan.BookTitle ?? "",
        BookAuthor = loan.BookAuthor ?? "",
        UserName = loan.UserName ?? "",
        LoanDate = loan.LoanDate,
        DueDate = loan.DueDate,
        ReturnDate = loan.ReturnDate,
        Status = loan.Status
    };
}
