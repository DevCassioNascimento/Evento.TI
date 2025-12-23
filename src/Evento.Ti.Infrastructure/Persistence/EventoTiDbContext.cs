// Sprint 4 - Relacionamento Evento-Ativo (EF Core mapping da entidade de junção)

using Evento.Ti.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Evento.Ti.Infrastructure.Persistence
{
    public class EventoTiDbContext : DbContext
    {
        public EventoTiDbContext(DbContextOptions<EventoTiDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Ativo> Ativos => Set<Ativo>();
        public DbSet<Evento.Ti.Domain.Entities.Event> Eventos => Set<Evento.Ti.Domain.Entities.Event>();

        // Sprint 4: entidade de junção
        public DbSet<EventAtivo> EventAtivos => Set<EventAtivo>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(builder =>
            {
                builder.ToTable("users");

                builder.HasKey(u => u.Id);

                builder.Property(u => u.Name)
                    .HasMaxLength(150)
                    .IsRequired();

                builder.Property(u => u.Email)
                    .HasMaxLength(200)
                    .IsRequired();

                builder.HasIndex(u => u.Email)
                    .IsUnique();

                builder.Property(u => u.PasswordHash)
                    .HasMaxLength(500)
                    .IsRequired();

                builder.Property(u => u.Role)
                    .IsRequired();

                builder.Property(u => u.IsActive)
                    .HasDefaultValue(true);

                builder.Property(u => u.CreatedAt)
                    .IsRequired();

                builder.Property(u => u.UpdatedAt);
            });

            // Mapping: Ativo -> tabela "ativos"
            modelBuilder.Entity<Ativo>(builder =>
            {
                builder.ToTable("ativos");

                builder.HasKey(a => a.Id);

                builder.Property(a => a.Name)
                    .HasMaxLength(200)
                    .IsRequired();

                builder.Property(a => a.Tag)
                    .HasMaxLength(50);

                builder.Property(a => a.SerialNumber)
                    .HasMaxLength(100);

                // Enum armazenado como int
                builder.Property(a => a.Status)
                    .IsRequired();

                builder.Property(a => a.CreatedAt)
                    .IsRequired();

                builder.Property(a => a.UpdatedAt);
            });

            modelBuilder.Entity<Evento.Ti.Domain.Entities.Event>(builder =>
            {
                builder.ToTable("eventos");

                builder.HasKey(e => e.Id);

                builder.Property(e => e.Titulo)
                    .HasMaxLength(200)
                    .IsRequired();

                builder.Property(e => e.Descricao)
                    .HasMaxLength(1000);

                builder.Property(e => e.Data)
                    .IsRequired();

                builder.Property(e => e.Local)
                    .HasMaxLength(200);

                builder.Property(e => e.DepartamentoResponsavel)
                    .HasMaxLength(150)
                    .IsRequired();

                builder.Property(e => e.CreatedAt)
                    .IsRequired();

                builder.Property(e => e.UpdatedAt);
            });

            // Sprint 4: Mapping EventAtivo -> tabela de junção
            modelBuilder.Entity<EventAtivo>(builder =>
            {
                builder.ToTable("eventos_ativos");

                // PK composta
                builder.HasKey(x => new { x.EventId, x.AtivoId });

                // Relacionamento com Event
                builder.HasOne(x => x.Event)
                    .WithMany(e => e.Ativos)
                    .HasForeignKey(x => x.EventId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relacionamento com Ativo
                builder.HasOne(x => x.Ativo)
                    .WithMany(a => a.Events)
                    .HasForeignKey(x => x.AtivoId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Sprint 4: status "separado para o evento"
                builder.Property(x => x.IsSeparado)
                    .IsRequired()
                    .HasDefaultValue(false);

                builder.Property(x => x.CreatedAt)
                    .IsRequired();

                builder.Property(x => x.UpdatedAt);

                // (Opcional, mas útil) índices de apoio
                builder.HasIndex(x => x.AtivoId);
                builder.HasIndex(x => x.EventId);
            });
        }
    }
}
