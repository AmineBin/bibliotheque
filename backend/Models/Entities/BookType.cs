namespace Bibliotheque.Api.Models.Entities;

public class BookType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MinAccessLevel { get; set; } = 1;
}
