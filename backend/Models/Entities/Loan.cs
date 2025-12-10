namespace Bibliotheque.Api.Models.Entities;

public class Loan
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int BookId { get; set; }
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string Status { get; set; } = "active";
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties pour les jointures
    public string? BookTitle { get; set; }
    public string? BookAuthor { get; set; }
    public string? UserName { get; set; }
}
