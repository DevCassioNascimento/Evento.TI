using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Evento.Ti.Infrastructure.Authentication;
using Evento.TI.Application.Common.Interfaces.Authentication;
using Evento.Ti.Infrastructure.Persistence;
using Evento.Ti.Domain.Entities;
using Evento.Ti.Application.Auth;
using Evento.Ti.Infrastructure.Auth;
using Evento.Ti.Application.Events.Create;
using Evento.Ti.Infrastructure.Events.Create;

namespace Evento.Ti.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ======================================
            // BANCO DE DADOS (PostgreSQL)
            // ======================================
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<EventoTiDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

            // ======================================
            // AUTENTICAÇÃO / SEGURANÇA
            // ======================================

            // Hash de senha usando Identity
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            // Serviço de autenticação (login, etc.)
            services.AddScoped<IAuthService, AuthService>();

            // Configurações tipadas de JWT a partir do appsettings.json ("JwtSettings")
            services.Configure<JwtSettings>(
                configuration.GetSection(JwtSettings.SectionName));

            // Recupera JwtSettings para configurar o JwtBearer
            var jwtSettings = configuration
                .GetSection(JwtSettings.SectionName)
                .Get<JwtSettings>();

            if (jwtSettings is null)
            {
                throw new InvalidOperationException(
                    "As configurações de JwtSettings não foram encontradas. " +
                    "Verifique se a seção \"JwtSettings\" está definida no appsettings.json.");
            }

            var keyBytes = Encoding.UTF8.GetBytes(jwtSettings.Secret);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            // Gerador de tokens JWT (stateless, pode ser Singleton)
            services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

            // Registro da autenticação baseada em JWT Bearer
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = signingKey,

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(1)
                    };
                });

            // Sistema de autorização (roles, policies etc.)
            services.AddAuthorization();

            // ======================================
            // Aqui depois registraremos repositórios,
            // UnitOfWork, serviços adicionais, etc.
            // ======================================
            services.AddScoped<ICreateEventService, CreateEventService>();

            return services;
        }
    }
}
