using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Bibliotheque.Api.Data;
using Bibliotheque.Api.Models.DTOs;
using Bibliotheque.Api.Repositories;
using Bibliotheque.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddSingleton<DbContext>();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<ILoanRepository, LoanRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IOverdueNotificationService, OverdueNotificationService>();

// Background Service pour les rappels (activé uniquement en production)
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<ReminderBackgroundService>();
}

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Politique pour les bibliothécaires uniquement
    options.AddPolicy("LibrarianOnly", policy =>
        policy.RequireClaim(ClaimTypes.Role, "Librarian"));
    
    // Politique pour les utilisateurs authentifiés
    options.AddPolicy("Authenticated", policy =>
        policy.RequireAuthenticatedUser());
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Health check
app.MapGet("/status", () => new { status = "OK", timestamp = DateTime.UtcNow });

// ===================
// AUTH ENDPOINTS
// ===================

app.MapPost("/auth/login", async (LoginRequest request, IAuthService authService) =>
{
    var result = await authService.LoginAsync(request);
    return result is null 
        ? Results.Unauthorized() 
        : Results.Ok(result);
}).WithTags("Auth");

app.MapPost("/auth/register", async (RegisterRequest request, IAuthService authService) =>
{
    var result = await authService.RegisterAsync(request);
    return result is null 
        ? Results.BadRequest(new { message = "Email already exists" }) 
        : Results.Created($"/users/{result.User.Id}", result);
}).WithTags("Auth");

// ===================
// BOOKS ENDPOINTS
// ===================

// Endpoint pour les admins - tous les livres sans filtrage
app.MapGet("/books", async (string? search, IBookService bookService) =>
{
    var books = string.IsNullOrEmpty(search) 
        ? await bookService.GetAllAsync() 
        : await bookService.SearchAsync(search);
    return Results.Ok(books);
}).WithTags("Books");

// Endpoint pour les utilisateurs - filtre les livres selon le niveau d'accès
app.MapGet("/books/user", async (string? search, ClaimsPrincipal user, IBookService bookService) =>
{
    var accessLevelClaim = user.FindFirst("AccessLevel")?.Value;
    var accessLevel = 1; // Par défaut niveau étudiant
    if (accessLevelClaim != null && int.TryParse(accessLevelClaim, out var level))
    {
        accessLevel = level;
    }
    
    var books = string.IsNullOrEmpty(search)
        ? await bookService.GetAllForUserAsync(accessLevel)
        : await bookService.SearchForUserAsync(search, accessLevel);
    return Results.Ok(books);
}).RequireAuthorization().WithTags("Books");

// Endpoint pour récupérer tous les types de livres
app.MapGet("/books/types", async (IBookService bookService) =>
{
    var types = await bookService.GetAllTypesAsync();
    return Results.Ok(types);
}).WithTags("Books");

app.MapGet("/books/{id:int}", async (int id, IBookService bookService) =>
{
    var book = await bookService.GetByIdAsync(id);
    return book is null ? Results.NotFound() : Results.Ok(book);
}).WithTags("Books");

app.MapPost("/books", async (CreateBookRequest request, IBookService bookService) =>
{
    var book = await bookService.CreateAsync(request);
    return Results.Created($"/books/{book.Id}", book);
}).RequireAuthorization("LibrarianOnly").WithTags("Books");

app.MapPut("/books/{id:int}", async (int id, UpdateBookRequest request, IBookService bookService) =>
{
    var book = await bookService.UpdateAsync(id, request);
    return book is null ? Results.NotFound() : Results.Ok(book);
}).RequireAuthorization("LibrarianOnly").WithTags("Books");

app.MapDelete("/books/{id:int}", async (int id, IBookService bookService) =>
{
    var deleted = await bookService.DeleteAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
}).RequireAuthorization("LibrarianOnly").WithTags("Books");

// ===================
// IMAGES ENDPOINTS
// ===================

app.MapPost("/books/{id:int}/image", async (int id, IFormFile file, IBookService bookService, IImageService imageService) =>
{
    try
    {
        var book = await bookService.GetByIdAsync(id);
        if (book is null) return Results.NotFound();

        // Supprimer l'ancienne image si elle existe
        if (!string.IsNullOrEmpty(book.ImagePath))
        {
            await imageService.DeleteImageAsync(book.ImagePath);
        }

        // Sauvegarder la nouvelle image
        var fileName = await imageService.SaveImageAsync(id, file);
        
        // Mettre à jour le livre avec le chemin de l'image
        var updateRequest = new UpdateBookRequest
        {
            Title = book.Title,
            Author = book.Author,
            Isbn = book.Isbn,
            Description = book.Description,
            PublicationYear = book.PublicationYear,
            Availability = book.Availability,
            ImagePath = fileName
        };
        
        var updated = await bookService.UpdateAsync(id, updateRequest);
        return Results.Ok(new { imagePath = fileName });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
}).RequireAuthorization("LibrarianOnly").DisableAntiforgery().WithTags("Images");

app.MapGet("/images/books/{fileName}", async (string fileName, IImageService imageService) =>
{
    try
    {
        var (stream, contentType) = await imageService.GetImageAsync(fileName);
        return Results.Stream(stream, contentType);
    }
    catch (FileNotFoundException)
    {
        return Results.NotFound();
    }
}).WithTags("Images");

app.MapDelete("/books/{id:int}/image", async (int id, IBookService bookService, IImageService imageService) =>
{
    var book = await bookService.GetByIdAsync(id);
    if (book is null) return Results.NotFound();
    
    if (string.IsNullOrEmpty(book.ImagePath))
        return Results.NoContent();

    await imageService.DeleteImageAsync(book.ImagePath);
    
    var updateRequest = new UpdateBookRequest
    {
        Title = book.Title,
        Author = book.Author,
        Isbn = book.Isbn,
        Description = book.Description,
        PublicationYear = book.PublicationYear,
        Availability = book.Availability,
        ImagePath = null
    };
    
    await bookService.UpdateAsync(id, updateRequest);
    return Results.NoContent();
}).RequireAuthorization("LibrarianOnly").WithTags("Images");

// ===================
// LOANS ENDPOINTS
// ===================

app.MapGet("/loans", async (ILoanService loanService) =>
{
    var loans = await loanService.GetAllAsync();
    return Results.Ok(loans);
}).RequireAuthorization("LibrarianOnly").WithTags("Loans");

app.MapGet("/loans/my", async (ClaimsPrincipal user, ILoanService loanService) =>
{
    var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
    {
        return Results.Unauthorized();
    }
    var loans = await loanService.GetByUserIdAsync(userId);
    return Results.Ok(loans);
}).RequireAuthorization().WithTags("Loans");

app.MapPost("/loans", async (CreateLoanRequest request, ClaimsPrincipal user, ILoanService loanService) =>
{
    var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
    {
        return Results.Unauthorized();
    }
    var loan = await loanService.BorrowBookAsync(userId, request.BookId);
    return loan is null 
        ? Results.BadRequest(new { message = "Book not available or already borrowed" }) 
        : Results.Created($"/loans/{loan.Id}", loan);
}).RequireAuthorization().WithTags("Loans");

app.MapPost("/loans/{id:int}/return", async (int id, ClaimsPrincipal user, ILoanService loanService) =>
{
    var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
    {
        return Results.Unauthorized();
    }
    var returned = await loanService.ReturnBookAsync(id, userId);
    return returned 
        ? Results.Ok(new { message = "Book returned successfully" }) 
        : Results.BadRequest(new { message = "Could not return book" });
}).RequireAuthorization().WithTags("Loans");

// ===================
// DASHBOARD ENDPOINT
// ===================

app.MapGet("/dashboard/stats", async (ILoanService loanService) =>
{
    var stats = await loanService.GetDashboardStatsAsync();
    return Results.Ok(stats);
}).RequireAuthorization("LibrarianOnly").WithTags("Dashboard");

// ===================
// NOTIFICATIONS / REMINDERS ENDPOINTS
// ===================

app.MapGet("/reminders/upcoming", async (int? days, IOverdueNotificationService notificationService) =>
{
    var daysToCheck = days ?? 30;
    var loans = await notificationService.GetUpcomingDueLoansAsync(daysToCheck);
    return Results.Ok(loans);
}).RequireAuthorization("LibrarianOnly").WithTags("Reminders");

app.MapGet("/reminders/overdue", async (IOverdueNotificationService notificationService) =>
{
    var loans = await notificationService.GetOverdueLoansAsync();
    return Results.Ok(loans);
}).RequireAuthorization("LibrarianOnly").WithTags("Reminders");

app.MapGet("/reminders/history", async (int? loanId, IOverdueNotificationService notificationService) =>
{
    var history = await notificationService.GetReminderHistoryAsync(loanId);
    return Results.Ok(history);
}).RequireAuthorization("LibrarianOnly").WithTags("Reminders");

// Endpoint pour déclencher manuellement l'envoi des rappels (pour les tests)
app.MapPost("/reminders/send", async (IOverdueNotificationService notificationService, ILogger<Program> logger) =>
{
    var sent = new List<object>();
    
    // Rappels J-30
    var loans30 = await notificationService.GetUpcomingDueLoansAsync(30);
    foreach (var loan in loans30.Where(l => l.DaysUntilDue >= 28 && l.DaysUntilDue <= 30))
    {
        await notificationService.LogReminderSentAsync(loan.LoanId, "J-30");
        sent.Add(new { loan.LoanId, loan.UserName, loan.BookTitle, Type = "J-30" });
    }
    
    // Rappels J-5
    var loans5 = await notificationService.GetUpcomingDueLoansAsync(5);
    foreach (var loan in loans5.Where(l => l.DaysUntilDue >= 3 && l.DaysUntilDue <= 5))
    {
        await notificationService.LogReminderSentAsync(loan.LoanId, "J-5");
        sent.Add(new { loan.LoanId, loan.UserName, loan.BookTitle, Type = "J-5" });
    }
    
    // Rappels retards
    var overdueLoans = await notificationService.GetOverdueLoansAsync();
    foreach (var loan in overdueLoans)
    {
        await notificationService.LogReminderSentAsync(loan.LoanId, "OVERDUE");
        sent.Add(new { loan.LoanId, loan.UserName, loan.BookTitle, Type = "OVERDUE", loan.DaysOverdue });
    }
    
    return Results.Ok(new { message = $"Sent {sent.Count} reminders", reminders = sent });
}).RequireAuthorization("LibrarianOnly").WithTags("Reminders");

app.Run();
