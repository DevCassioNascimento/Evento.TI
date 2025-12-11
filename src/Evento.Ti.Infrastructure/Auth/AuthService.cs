using Evento.Ti.Application.Auth;
using Evento.Ti.Domain.Entities;
using Evento.Ti.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Evento.Ti.Infrastructure.Auth
{
    public class AuthService : IAuthService
    {
        private readonly EventoTiDbContext _dbContext;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthService(EventoTiDbContext dbContext, IPasswordHasher<User> passwordHasher)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
        }

        public async Task<LoginResultDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (user is null)
            {
                return LoginResultDto.Fail("Credenciais inválidas.");
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return LoginResultDto.Fail("Credenciais inválidas.");
            }

            return LoginResultDto.Ok(user.Id, user.Name, user.Email, user.Role.ToString());
        }
    }
}
