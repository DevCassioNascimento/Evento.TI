// Sprint 4 - Relacionamento Evento-Ativo (navegação para entidade de junção)

using System.Collections.Generic;

namespace Evento.Ti.Domain.Entities
{
    public class Event
    {
        // Chave primária
        public Guid Id { get; private set; }

        // Dados do evento
        public string Titulo { get; private set; } = null!;
        public string? Descricao { get; private set; }
        public DateTime Data { get; private set; }
        public string? Local { get; private set; }
        public string DepartamentoResponsavel { get; private set; } = null!;

        // Sprint 4: relacionamento Evento-Ativo (entidade de junção)
        public ICollection<EventAtivo> Ativos { get; private set; } = new List<EventAtivo>();

        // Auditoria (padrão do projeto)
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // Construtor para EF
        private Event() { }

        public Event(
            string titulo,
            string? descricao,
            DateTime data,
            string? local,
            string departamentoResponsavel)
        {
            Id = Guid.NewGuid();

            Titulo = titulo;
            Descricao = descricao;
            Data = data;
            Local = local;
            DepartamentoResponsavel = departamentoResponsavel;

            CreatedAt = DateTime.UtcNow;
            UpdatedAt = null;
        }

        public void Update(
            string titulo,
            string? descricao,
            DateTime data,
            string? local,
            string departamentoResponsavel)
        {
            Titulo = titulo;
            Descricao = descricao;
            Data = data;
            Local = local;
            DepartamentoResponsavel = departamentoResponsavel;

            UpdatedAt = DateTime.UtcNow;
        }
    }
}
