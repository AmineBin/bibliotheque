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

builder.Services.AddAuthorization();

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

app.MapGet("/books", async (string? search, IBookService bookService) =>
{
    var books = string.IsNullOrEmpty(search) 
        ? await bookService.GetAllAsync() 
        : await bookService.SearchAsync(search);
    return Results.Ok(books);
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
}).RequireAuthorization().WithTags("Books");

app.MapPut("/books/{id:int}", async (int id, UpdateBookRequest request, IBookService bookService) =>
{
    var book = await bookService.UpdateAsync(id, request);
    return book is null ? Results.NotFound() : Results.Ok(book);
}).RequireAuthorization().WithTags("Books");

app.MapDelete("/books/{id:int}", async (int id, IBookService bookService) =>
{
    var deleted = await bookService.DeleteAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
}).RequireAuthorization().WithTags("Books");

// ===================
// LOANS ENDPOINTS
// ===================

app.MapGet("/loans", async (ILoanService loanService) =>
{
    var loans = await loanService.GetAllAsync();
    return Results.Ok(loans);
}).RequireAuthorization().WithTags("Loans");

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
}).RequireAuthorization().WithTags("Dashboard");

app.Run();
