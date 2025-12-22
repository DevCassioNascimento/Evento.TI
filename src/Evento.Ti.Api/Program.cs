using Evento.Ti.Application.Auth;
using Evento.Ti.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Evento.TI.Application.Common.Interfaces.Authentication;
using Evento.Ti.Domain.Entities;
using Evento.Ti.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Evento.Ti.Application.Events.Create;
using Microsoft.Extensions.DependencyInjection;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// criando usuario para subir junto com o banco em dev Inico

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var db = scope.ServiceProvider.GetRequiredService<EventoTiDbContext>();

    // Se o banco já foi migrado e não existe nenhum usuário, cria um Admin padrão
    if (!db.Users.Any())
    {
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        var admin = new User(
            name: "Admin",
            email: "admin@evento.ti",
            passwordHash: "TEMP",
            role: UserRole.Admin
        );

        var hash = hasher.HashPassword(admin, "Admin@123");
        admin.UpdatePassword(hash);

        db.Users.Add(admin);
        db.SaveChanges();
    }
}

// criando usuario para subir junto com o banco em dev FIM


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

// ==================================================================
// Sprint 2 – Inventário: CRUD + Status
// Endpoint específico: alterar SOMENTE o Status do Ativo
// PUT /api/ativos/{id}/status
// Body: { "status": "Disponivel" }
// ==================================================================
ativosGroup.MapPut("/{id:guid}/status", async (Guid id, UpdateAtivoStatusRequest request, EventoTiDbContext db) =>
{
    var ativo = await db.Ativos.FirstOrDefaultAsync(a => a.Id == id);
    if (ativo is null)
        return Results.NotFound();

    ativo.Status = request.Status;
    ativo.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();

    return Results.Ok(ativo);
})
.WithName("UpdateAtivoStatus");

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

var eventsGroup = app.MapGroup("/api/events")
    .RequireAuthorization();

eventsGroup.MapPost("/", async (
    CreateEventRequest request,
    ICreateEventService createEventService,
    CancellationToken ct) =>
{
    try
    {
        var id = await createEventService.CreateAsync(request, ct);
        return Results.Created($"/api/events/{id}", new { id });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("CreateEvent");

eventsGroup.MapGet("/", async (EventoTiDbContext db, CancellationToken ct) =>
{
    var items = await db.Eventos
        .AsNoTracking()
        .OrderByDescending(e => e.Data)
        .Select(e => new
        {
            e.Id,
            e.Titulo,
            e.Descricao,
            e.Data,
            e.Local,
            e.DepartamentoResponsavel
        })
        .ToListAsync(ct);

    return Results.Ok(items);
})
.WithName("ListEvents");





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

// Sprint 2 – Inventário: CRUD + Status
record UpdateAtivoStatusRequest(AtivoStatus Status);
