using Evento.Ti.Application.Auth;
using Evento.Ti.Infrastructure;

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

// ==============================
// Habilitar arquivos estáticos (wwwroot)
// ==============================
app.UseStaticFiles();

// ==============================
// Endpoint raiz ("/") -> index.html
// ==============================
app.MapGet("/", () => Results.Redirect("/index.html"));

// ==============================
// AUTH: endpoint de login
// POST /api/auth/login
// Body: { "email": "...", "password": "..." }
// ==============================
var authGroup = app.MapGroup("/api/auth");

authGroup.MapPost("/login", async (LoginRequestDto request, IAuthService authService) =>
{
    var result = await authService.LoginAsync(request);

    if (!result.Success)
        return Results.Unauthorized();

    // Por enquanto retornamos apenas os dados básicos do usuário.
    // Na próxima tarefa (JWT ou cookies) vamos adicionar token/session.
    return Results.Ok(result);
})
.WithName("Login");

// ==============================
// Endpoint de exemplo (weatherforecast)
// ==============================
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
