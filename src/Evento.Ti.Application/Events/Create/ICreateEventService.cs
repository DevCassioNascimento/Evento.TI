// Sprint 3 – Eventos (Cadastro + Lista)
namespace Evento.Ti.Application.Events.Create
{
    public interface ICreateEventService
    {
        Task<Guid> CreateAsync(CreateEventRequest request, CancellationToken ct = default);
    }
}
