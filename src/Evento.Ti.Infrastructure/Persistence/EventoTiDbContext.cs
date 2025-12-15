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





        }
    }
}
