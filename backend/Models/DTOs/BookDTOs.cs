using System.ComponentModel.DataAnnotations;

namespace Bibliotheque.Api.Models.DTOs;

public class CreateBookRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Author { get; set; } = string.Empty;
    
    public string? Isbn { get; set; }
    public string? Description { get; set; }
    public int? PublicationYear { get; set; }
    public string Availability { get; set; } = "available";
    public int TypeId { get; set; } = 1;
}

public class UpdateBookRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Author { get; set; } = string.Empty;
    
    public string? Isbn { get; set; }
    public string? Description { get; set; }
    public int? PublicationYear { get; set; }
    public string Availability { get; set; } = "available";
    public string? ImagePath { get; set; }
    public int TypeId { get; set; } = 1;
}

public class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? Isbn { get; set; }
    public string? Description { get; set; }
    public int? PublicationYear { get; set; }
    public string Availability { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public int TypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public int MinAccessLevel { get; set; }
}

public class BookTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MinAccessLevel { get; set; }
}
