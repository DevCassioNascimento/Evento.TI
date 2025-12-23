// Sprint 4 - Checklists de Ativos por Evento (endpoints checklist / separado)

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

eventsGroup.MapPut("/{id:guid}", async (Guid id, UpdateEventRequest request, EventoTiDbContext db, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Titulo))
        return Results.BadRequest(new { error = "Título é obrigatório." });

    if (string.IsNullOrWhiteSpace(request.DepartamentoResponsavel))
        return Results.BadRequest(new { error = "Departamento responsável é obrigatório." });

    var evt = await db.Eventos.FirstOrDefaultAsync(e => e.Id == id, ct);
    if (evt is null)
        return Results.NotFound(new { error = "Evento não encontrado." });

    var titulo = request.Titulo.Trim();
    var descricao = string.IsNullOrWhiteSpace(request.Descricao) ? null : request.Descricao.Trim();
    var local = string.IsNullOrWhiteSpace(request.Local) ? null : request.Local.Trim();
    var depto = request.DepartamentoResponsavel.Trim();

    evt.Update(titulo, descricao, request.Data, local, depto);

    await db.SaveChangesAsync(ct);

    return Results.Ok(new
    {
        evt.Id,
        evt.Titulo,
        evt.Descricao,
        evt.Data,
        evt.Local,
        evt.DepartamentoResponsavel
    });
})
.WithName("UpdateEvent");

eventsGroup.MapDelete("/{id:guid}", async (Guid id, EventoTiDbContext db, CancellationToken ct) =>
{
    // 1) Confere se evento existe
    var evt = await db.Eventos.FirstOrDefaultAsync(e => e.Id == id, ct);
    if (evt is null)
        return Results.NotFound(new { error = "Evento não encontrado." });

    // 2) Regra conservadora: não permitir excluir se houver ativos vinculados
    var hasLinks = await db.EventAtivos.AnyAsync(x => x.EventId == id, ct);
    if (hasLinks)
        return Results.Conflict(new { error = "Não é possível excluir: existem ativos vinculados ao evento." });

    db.Eventos.Remove(evt);
    await db.SaveChangesAsync(ct);

    return Results.NoContent();
})
.WithName("DeleteEvent");


// ==================================================================
// Sprint 4 – Checklists de Ativos por Evento
// 1) Listar ativos do evento (com IsSeparado)
// 2) Vincular ativo ao evento
// 3) Marcar/desmarcar IsSeparado
// 4) Remover vínculo
// ==================================================================

eventsGroup.MapGet("/{eventId:guid}/ativos", async (Guid eventId, EventoTiDbContext db, CancellationToken ct) =>
{
    var exists = await db.Eventos.AsNoTracking().AnyAsync(e => e.Id == eventId, ct);
    if (!exists)
        return Results.NotFound(new { error = "Evento não encontrado." });

    var itens = await db.EventAtivos
        .AsNoTracking()
        .Where(x => x.EventId == eventId)
        .Include(x => x.Ativo)
        .OrderBy(x => x.Ativo.Name)
        .Select(x => new
        {
            x.AtivoId,
            x.Ativo.Name,
            x.Ativo.Tag,
            x.Ativo.SerialNumber,
            x.Ativo.Status,
            x.IsSeparado
        })
        .ToListAsync(ct);

    return Results.Ok(itens);
})
.WithName("ListEventAtivosChecklist");

eventsGroup.MapPost("/{eventId:guid}/ativos/{ativoId:guid}", async (Guid eventId, Guid ativoId, EventoTiDbContext db, CancellationToken ct) =>
{
    var eventoExists = await db.Eventos.AsNoTracking().AnyAsync(e => e.Id == eventId, ct);
    if (!eventoExists)
        return Results.NotFound(new { error = "Evento não encontrado." });

    var ativoExists = await db.Ativos.AsNoTracking().AnyAsync(a => a.Id == ativoId, ct);
    if (!ativoExists)
        return Results.NotFound(new { error = "Ativo não encontrado." });

    var jaVinculado = await db.EventAtivos.AnyAsync(x => x.EventId == eventId && x.AtivoId == ativoId, ct);
    if (jaVinculado)
        return Results.Conflict(new { error = "Ativo já está vinculado a este evento." });

    var link = new EventAtivo(eventId, ativoId);

    db.EventAtivos.Add(link);
    await db.SaveChangesAsync(ct);

    return Results.Created($"/api/events/{eventId}/ativos/{ativoId}", new { eventId, ativoId, isSeparado = link.IsSeparado });
})
.WithName("AddAtivoToEvent");

eventsGroup.MapPut("/{eventId:guid}/ativos/{ativoId:guid}/separado", async (Guid eventId, Guid ativoId, UpdateSeparadoRequest request, EventoTiDbContext db, CancellationToken ct) =>
{
    var link = await db.EventAtivos.FirstOrDefaultAsync(x => x.EventId == eventId && x.AtivoId == ativoId, ct);
    if (link is null)
        return Results.NotFound(new { error = "Vínculo Evento-Ativo não encontrado." });

    link.MarcarSeparado(request.IsSeparado);
    await db.SaveChangesAsync(ct);

    return Results.Ok(new { eventId, ativoId, isSeparado = link.IsSeparado });
})
.WithName("UpdateChecklistSeparado");

eventsGroup.MapDelete("/{eventId:guid}/ativos/{ativoId:guid}", async (Guid eventId, Guid ativoId, EventoTiDbContext db, CancellationToken ct) =>
{
    var link = await db.EventAtivos.FirstOrDefaultAsync(x => x.EventId == eventId && x.AtivoId == ativoId, ct);
    if (link is null)
        return Results.NotFound(new { error = "Vínculo Evento-Ativo não encontrado." });

    db.EventAtivos.Remove(link);
    await db.SaveChangesAsync(ct);

    return Results.NoContent();
})
.WithName("RemoveAtivoFromEvent");

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

// Sprint 4 – Checklist: marcar separado
record UpdateSeparadoRequest(bool IsSeparado);

// Sprint 4 – Eventos: editar
record UpdateEventRequest(string Titulo, string? Descricao, DateTime Data, string? Local, string DepartamentoResponsavel);

