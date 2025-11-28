using System;
using System.Collections.Generic;
using Models;

public class BookManager
{
    public Guid Id { get; private set; }
    public List<Book> Books { get; private set; }

    private static readonly Lazy<BookManager> _instance = new Lazy<BookManager>(() => new BookManager());

    public static BookManager Instance => _instance.Value;

    private BookManager()
    {
        Id = Guid.NewGuid();
        Books = new List<Book>();
    }

    public void AddBook(Book book)
    {
        Books.Add(book);
    }

    public void RemoveBook(Book book)
    {
        Books.Remove(book);
    }
    
    public Book FindBookById(Guid id)
    {
        return Books.Find(book => book.Id == id);
    }
    
    public List<Book> FindBooksByTitle(string title)
    {
        return Books.FindAll(book => book.Title.IndexOf(title, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    public List<Book> FindBooksByAuthor(string author)
    {
        return Books.FindAll(book => book.Author.IndexOf(author, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    public List<Book> FindBooksByYear(int firstYear, int secondYear)
    {
        return Books.FindAll(book => book.PublicationDate.Year >= firstYear && book.PublicationDate.Year <= secondYear);    
    }
    
    public List<Book> ListBooks()
    {
        return Books;
    }
}