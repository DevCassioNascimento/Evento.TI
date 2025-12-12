using Evento.Ti.Application.Auth;
using Evento.Ti.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Evento.TI.Application.Common.Interfaces.Authentication;
using Evento.Ti.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Habilitar arquivos estáticos (wwwroot)
app.UseStaticFiles();

// Autenticação e Autorização via JWT
app.UseAuthentication();
app.UseAuthorization();

// ==================================================================
// Sprint 1 – Autenticação/Autorização (Login):
// Middleware simples de LOG para enxergar o caminho das requisições
// ==================================================================
app.Use(async (context, next) =>
{
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {context.Request.Method} {context.Request.Path}");
    await next();
});

// Endpoint raiz ("/") -> index.html
app.MapGet("/", () => Results.Redirect("/html/index.html"));

// ==================================================================
// Endpoint de teste simples, sem nada de auth, só para validar rota
// GET /test
// ==================================================================
app.MapGet("/test", () => Results.Ok("API ON - /test"));

// ===============================
// AUTH: endpoint de login (JWT)
// POST /api/auth/login
// Body: { "email": "...", "password": "..." }
// ===============================
var authGroup = app.MapGroup("/api/auth");

authGroup.MapPost("/login", async (
    LoginRequestDto request,
    IAuthService authService,
    IJwtTokenGenerator jwtTokenGenerator) =>
{
    // 1. Usa o serviço de autenticação para validar o usuário.
    var result = await authService.LoginAsync(request);

    if (!result.Success || result.UserId is null)
        return Results.Unauthorized();

    var token = jwtTokenGenerator.GenerateToken(
        result.UserId.Value,
        result.Name ?? string.Empty,
        result.Email ?? string.Empty,
        result.Role ?? string.Empty
    );

    var response = new
    {
        Id = result.UserId,
        result.Name,
        result.Email,
        result.Role,
        Token = token
    };

    return Results.Ok(response);
})
.WithName("Login");

// Endpoint de exemplo (weatherforecast)
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild",
    "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
