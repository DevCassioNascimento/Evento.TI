using Evento.Ti.Application.Auth;
using Evento.Ti.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Evento.TI.Application.Common.Interfaces.Authentication;
using Evento.Ti.Domain.Entities;
using Evento.Ti.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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

// ==================================================================
// Sprint 2 – Inventário: CRUD + Status
// Endpoints CRUD de Ativos
// Base: /api/ativos
// Obs: por ora, exige usuário autenticado (RequireAuthorization).
// ==================================================================
var ativosGroup = app.MapGroup("/api/ativos")
    .RequireAuthorization();

// DTOs locais (Sprint 2) para não expor a entidade diretamente no body


ativosGroup.MapPost("/", async (CreateAtivoRequest request, EventoTiDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
        return Results.BadRequest(new { error = "Name é obrigatório." });

    var ativo = new Ativo
    {
        Name = request.Name.Trim(),
        Tag = string.IsNullOrWhiteSpace(request.Tag) ? null : request.Tag.Trim(),
        SerialNumber = string.IsNullOrWhiteSpace(request.SerialNumber) ? null : request.SerialNumber.Trim(),
        Status = request.Status ?? AtivoStatus.Disponivel,
        CreatedAt = DateTime.UtcNow
    };

    db.Ativos.Add(ativo);
    await db.SaveChangesAsync();

    return Results.Created($"/api/ativos/{ativo.Id}", ativo);
})
.WithName("CreateAtivo");

ativosGroup.MapGet("/", async (EventoTiDbContext db) =>
{
    var ativos = await db.Ativos
        .AsNoTracking()
        .OrderBy(a => a.Name)
        .ToListAsync();

    return Results.Ok(ativos);
})
.WithName("ListAtivos");

ativosGroup.MapGet("/{id:guid}", async (Guid id, EventoTiDbContext db) =>
{
    var ativo = await db.Ativos
        .AsNoTracking()
        .FirstOrDefaultAsync(a => a.Id == id);

    return ativo is null ? Results.NotFound() : Results.Ok(ativo);
})
.WithName("GetAtivoById");

ativosGroup.MapPut("/{id:guid}", async (Guid id, UpdateAtivoRequest request, EventoTiDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
        return Results.BadRequest(new { error = "Name é obrigatório." });

    var ativo = await db.Ativos.FirstOrDefaultAsync(a => a.Id == id);
    if (ativo is null)
        return Results.NotFound();

    ativo.Name = request.Name.Trim();
    ativo.Tag = string.IsNullOrWhiteSpace(request.Tag) ? null : request.Tag.Trim();
    ativo.SerialNumber = string.IsNullOrWhiteSpace(request.SerialNumber) ? null : request.SerialNumber.Trim();
    ativo.Status = request.Status;
    ativo.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();
    return Results.Ok(ativo);
})
.WithName("UpdateAtivo");

ativosGroup.MapDelete("/{id:guid}", async (Guid id, EventoTiDbContext db) =>
{
    var ativo = await db.Ativos.FirstOrDefaultAsync(a => a.Id == id);
    if (ativo is null)
        return Results.NotFound();

    db.Ativos.Remove(ativo);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.WithName("DeleteAtivo");




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

record CreateAtivoRequest(string Name, string? Tag, string? SerialNumber, AtivoStatus? Status);
record UpdateAtivoRequest(string Name, string? Tag, string? SerialNumber, AtivoStatus Status);