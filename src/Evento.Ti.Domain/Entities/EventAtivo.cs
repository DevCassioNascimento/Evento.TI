// Sprint 4 - Status "separado para o evento" no vínculo Evento-Ativo

using System;

namespace Evento.Ti.Domain.Entities
{
    public class EventAtivo
    {
        // Chaves (FKs)
        public Guid EventId { get; private set; }
        public Guid AtivoId { get; private set; }

        // Status do checklist (por evento)
        public bool IsSeparado { get; private set; }

        // Navegação
        public Event Event { get; private set; } = null!;
        public Ativo Ativo { get; private set; } = null!;

        // Auditoria básica do vínculo
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // Construtor para EF
        private EventAtivo() { }

        public EventAtivo(Guid eventId, Guid ativoId)
        {
            EventId = eventId;
            AtivoId = ativoId;

            IsSeparado = false;

            CreatedAt = DateTime.UtcNow;
            UpdatedAt = null;
        }

        public void MarcarSeparado(bool separado)
        {
            IsSeparado = separado;
            Touch();
        }

        public void Touch()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
