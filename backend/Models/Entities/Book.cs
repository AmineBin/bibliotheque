namespace Bibliotheque.Api.Models.Entities;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? Isbn { get; set; }
    public string? Description { get; set; }
    public int? PublicationYear { get; set; }
    public string Availability { get; set; } = "available";
    public string? ImagePath { get; set; }
    public int TypeId { get; set; } = 1;
    public string? TypeName { get; set; }
    public int MinAccessLevel { get; set; } = 1;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
