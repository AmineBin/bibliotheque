using System;

public class Book
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    public DateTime PublicationDate { get; set; }
    public enum Availability
    {
        InStock,
        Reserved,
        OutOfStock,
        NotYetPublished, // For future books
        Discontinued, // No longer available and will probably never be
    }

    public byte[] ImageData { get; set; }
    public string ImageMimeType { get; set; }
    
    public Book(){ }
    
    public Book(Guid Id, string Title, string Author, DateTime PublicationDate)
    {
        this.Id = Id;
        this.Title = Title;
        this.Author = Author;
        this.PublicationDate = PublicationDate;
    }
}