using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Evento.Ti.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Evento.Ti.Domain.Entities;
using Evento.Ti.Application.Auth;
using Evento.Ti.Infrastructure.Auth;

namespace Evento.Ti.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Lê a connection string do appsettings
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<EventoTiDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

            // Hash de senha
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            // Serviço de autenticação
            services.AddScoped<IAuthService, AuthService>();

            // Aqui depois registraremos repositórios, UnitOfWork, etc.

            return services;
        }
    }
}
