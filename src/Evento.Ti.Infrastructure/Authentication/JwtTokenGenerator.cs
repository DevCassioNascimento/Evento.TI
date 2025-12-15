using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Evento.TI.Application.Common.Interfaces.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Evento.Ti.Infrastructure.Authentication
{
    /// <summary>
    /// Implementação responsável por gerar tokens JWT com base nas configurações definidas em JwtSettings.
    /// </summary>
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtSettings _jwtSettings;

        /// <summary>
        /// Construtor que recebe as configurações de JWT via IOptions.
        /// </summary>
        /// <param name="jwtOptions">Opções tipadas de JwtSettings.</param>
        public JwtTokenGenerator(IOptions<JwtSettings> jwtOptions)
        {
            _jwtSettings = jwtOptions.Value;
        }

        /// <summary>
        /// Gera um token JWT incluindo as claims básicas do usuário.
        /// </summary>
        public string GenerateToken(Guid userId, string name, string email, string role)
        {
            // 1. Cria a lista de claims (informações) que irão dentro do token.
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId.ToString()),  // Identificador do usuário
                new(JwtRegisteredClaimNames.UniqueName, name),        // Nome do usuário
                new(JwtRegisteredClaimNames.Email, email),            // E-mail
                new(ClaimTypes.Role, role)                            // Papel do usuário no sistema
            };

            // 2. Cria a chave de segurança a partir do Secret configurado.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));

            // 3. Define o algoritmo de assinatura (HMAC-SHA256).
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 4. Calcula a data de expiração do token.
            var expiry = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

            // 5. Monta o token JWT.
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiry,
                signingCredentials: signingCredentials
            );

            // 6. Escreve o token em formato string (compact serialization).
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
