
namespace Evento.Ti.Domain.Entities
{
    public class Ativo
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Nome visível do ativo (ex: "Notebook Dell 5420")
        public string Name { get; set; } = string.Empty;

        // Opcional: patrimônio/etiqueta interna (ex: "APSE-000123")
        public string? Tag { get; set; }

        // Opcional: número de série
        public string? SerialNumber { get; set; }

        // Status do inventário (controle operacional)
        public AtivoStatus Status { get; set; } = AtivoStatus.Disponivel;

        // Sprint 4: relacionamento Evento-Ativo (entidade de junção)
        public ICollection<EventAtivo> Events { get; set; } = new List<EventAtivo>();

        // Auditoria básica
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    public enum AtivoStatus
    {
        Disponivel = 1,
        EmUso = 2,
        EmManutencao = 3,
        Quebrado = 4,
        Comprando = 5,
        Precisamos = 6,
        Emprestado = 7,
        Reservado = 8

    }
}
