// Sprint 3 – Eventos (Cadastro + Lista)
using Evento.Ti.Application.Events.Create;
using Evento.Ti.Domain.Entities;
using Evento.Ti.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Evento.Ti.Infrastructure.Events.Create
{
    public sealed class CreateEventService : ICreateEventService
    {
        private readonly EventoTiDbContext _db;

        public CreateEventService(EventoTiDbContext db)
        {
            _db = db;
        }

        public async Task<Guid> CreateAsync(CreateEventRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.Titulo))
                throw new ArgumentException("Título é obrigatório.", nameof(request.Titulo));

            if (string.IsNullOrWhiteSpace(request.DepartamentoResponsavel))
                throw new ArgumentException("Departamento responsável é obrigatório.", nameof(request.DepartamentoResponsavel));

            var entity = new Event(
                titulo: request.Titulo.Trim(),
                descricao: request.Descricao?.Trim(),
                data: request.Data,
                local: request.Local?.Trim(),
                departamentoResponsavel: request.DepartamentoResponsavel.Trim()
            );

            _db.Eventos.Add(entity);
            await _db.SaveChangesAsync(ct);

            return entity.Id;
        }
    }
}
