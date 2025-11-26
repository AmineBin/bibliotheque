using System.Collections.Generic;
var builder = WebApplication.CreateBuilder(args);

// TODO: enregistrer ici tes services (DbContext, repositories, etc.)

var app = builder.Build();

// TODO: configurer ici le pipeline HTTP (middlewares, etc.)
if (app.Environment.IsDevelopment())
{
    // ex : Swagger, pages de debug, etc.
}

app.UseHttpsRedirection();

// endpoints API (MapGet/MapPost/MapGroup, contrôleurs, ...)
app.MapGet("/api/status", () => new { status = "OK", timestamp = DateTime.UtcNow });
app.MapGet("/livres", () => new[] { "Livre 1", "Livre 2", "Livre 3" });
app.MapPost("/livres", (string titre) => $"Livre '{titre}' ajouté.");

app.Run();
