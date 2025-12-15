namespace Evento.Ti.Infrastructure.Authentication
{
    /// <summary>
    /// Representa as configurações de JWT, mapeadas a partir da seção "JwtSettings" do appsettings.json.
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// Nome da seção no appsettings.json.
        /// </summary>
        public const string SectionName = "JwtSettings";

        /// <summary>
        /// Chave secreta usada para assinar o token (deve ser protegida).
        /// </summary>
        public string Secret { get; init; } = string.Empty;

        /// <summary>
        /// Emissor do token (Issuer).
        /// </summary>
        public string Issuer { get; init; } = string.Empty;

        /// <summary>
        /// Público-alvo do token (Audience).
        /// </summary>
        public string Audience { get; init; } = string.Empty;

        /// <summary>
        /// Tempo de expiração do token em minutos.
        /// </summary>
        public int ExpiryMinutes { get; init; }
    }
}
