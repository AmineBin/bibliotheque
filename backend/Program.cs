using System.Collections.Generic;
using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Models;
using Services;

var builder = WebApplication.CreateBuilder(args);

// Dossier images (montez un volume vers '/app/data/images' en Docker)
var imagesFolder = "/app/data/images";
builder.Services.AddSingleton(new ImageService(imagesFolder));

builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseHttpsRedirection();

// Status
app.MapGet("/status", () => new { status = "OK", timestamp = DateTime.UtcNow });

// GET /books
app.MapGet("/books", async (LibraryContext db) =>
{
    var books = await db.Books.ToListAsync();
    return Results.Ok(books);
});

// GET /books/{id}
app.MapGet("/books/{id:guid}", async (Guid id, LibraryContext db) =>
{
    var book = await db.Books.FindAsync(id);
    return book is null ? Results.NotFound() : Results.Ok(book);
});

// POST /books
app.MapPost("/books", async (Book newBook, LibraryContext db) =>
{
    if (newBook.Id == Guid.Empty)
    {
        newBook.Id = Guid.NewGuid();
    }

    db.Books.Add(newBook);
    await db.SaveChangesAsync();
    return Results.Created($"/books/{newBook.Id}", newBook);
});

// DELETE /books/{id}
app.MapDelete("/books/{id:guid}", async (Guid id, LibraryContext db) =>
{
    var book = await db.Books.FindAsync(id);
    if (book is null) return Results.NotFound();

    db.Books.Remove(book);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Upload image
app.MapPost("/images/{livreId:guid}", async (Guid livreId, IFormFile file, ImageService imageService) =>
    {
        if (file == null || file.Length == 0) return Results.BadRequest("Fichier manquant");
        var fileName = await imageService.SaveToDiskAsync(livreId, file);
        return Results.Ok(new { fileName });
    })
    .Accepts<IFormFile>("multipart/form-data");

// Get image
app.MapGet("/images/{fileName}", async (string fileName, ImageService imageService) =>
{
    try
    {
        var (stream, contentType) = await imageService.GetStreamAsync(fileName);
		return Results.File(fileStream: stream, contentType: contentType, fileDownloadName: fileName);
    }
    catch (FileNotFoundException)
    {
        return Results.NotFound();
    }
});

app.Run();
