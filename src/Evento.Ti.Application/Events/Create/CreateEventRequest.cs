// Sprint 3 – Eventos (Cadastro + Lista)
namespace Evento.Ti.Application.Events.Create
{
    public sealed record CreateEventRequest(
        string Titulo,
        string? Descricao,
        DateTime Data,
        string? Local,
        string DepartamentoResponsavel
    );
}
