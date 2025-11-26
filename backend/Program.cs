using System.Collections.Generic;
using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services;

var builder = WebApplication.CreateBuilder(args);

// Dossier images (montez un volume vers '/app/data/images' en Docker)
var imagesFolder = "/app/data/images";
builder.Services.AddSingleton(new ImageService(imagesFolder));

var app = builder.Build();

app.UseHttpsRedirection();

// Status
app.MapGet("/status", () => new { status = "OK", timestamp = DateTime.UtcNow });

var bookManager = BookManager.Instance;

// GET /books
app.MapGet("/books", () =>
{
    return Results.Ok(bookManager.ListBooks());
});

// GET /books/{id}
app.MapGet("/books/{id:guid}", (Guid id) =>
{
    var book = bookManager.FindBookById(id);
    return book is null ? Results.NotFound() : Results.Ok(book);
});

// POST /books
app.MapPost("/books", (Book newBook) =>
{
    if (newBook.Id == Guid.Empty)
    {
        newBook.Id = Guid.NewGuid();
    }

    bookManager.AddBook(newBook);
    return Results.Created($"/books/{newBook.Id}", newBook);
});

// DELETE /books/{id}
app.MapDelete("/books/{id:guid}", (Guid id) =>
{
    var book = bookManager.FindBookById(id);
    if (book is null) return Results.NotFound();

    bookManager.RemoveBook(book);
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
