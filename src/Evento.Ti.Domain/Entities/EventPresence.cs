// Sprint 5 - Presença da Equipe

namespace Evento.Ti.Domain.Entities
{
    // 1 registro por usuário por evento (PK composta: EventId + UserId)
    public class EventPresence
    {
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }

        public PresenceStatus Status { get; set; } = PresenceStatus.Confirmed;
        public string? Reason { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navegações (mantém o mesmo estilo do seu Domain atual)
        public Event Event { get; set; } = default!;
        public User User { get; set; } = default!;
    }

    public enum PresenceStatus
    {
        Confirmed = 1,
        Declined = 2,
        Late = 3
    }
}
