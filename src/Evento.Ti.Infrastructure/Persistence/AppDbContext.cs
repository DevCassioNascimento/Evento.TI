using Microsoft.EntityFrameworkCore;

namespace Evento.Ti.Infrastructure.Persistence
{
    // DbContext principal da aplicação
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // DbSets das entidades vão entrar aqui futuramente
        // Exemplo (quando modelarmos):
        // public DbSet<Evento> Eventos { get; set; } = null!;
        // public DbSet<Ativo> Ativos { get; set; } = null!;
        // public DbSet<MembroEquipe> MembrosEquipe { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aqui depois adicionaremos as configurações de mapeamento (Fluent API)
            // modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
