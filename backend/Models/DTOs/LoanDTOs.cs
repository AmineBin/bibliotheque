namespace Bibliotheque.Api.Models.DTOs;

public class CreateLoanRequest
{
    public int BookId { get; set; }
}

public class LoanDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string BookAuthor { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class DashboardStatsDto
{
    public int TotalBooks { get; set; }
    public int AvailableBooks { get; set; }
    public int ActiveLoans { get; set; }
    public int OverdueLoans { get; set; }
    public int TotalUsers { get; set; }
    public List<PopularBookDto> PopularBooks { get; set; } = new();
}

public class PopularBookDto
{
    public string Title { get; set; } = string.Empty;
    public int LoanCount { get; set; }
}
