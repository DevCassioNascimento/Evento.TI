namespace Evento.TI.Application.Common.Interfaces.Authentication
{
    /// <summary>
    /// Responsável por gerar tokens JWT para usuários autenticados.
    /// Implementação concreta ficará na camada Infrastructure.
    /// </summary>
    public interface IJwtTokenGenerator
    {
        /// <summary>
        /// Gera um token JWT com as informações básicas do usuário.
        /// </summary>
        /// <param name="userId">Identificador único do usuário.</param>
        /// <param name="name">Nome do usuário.</param>
        /// <param name="email">E-mail do usuário.</param>
        /// <param name="role">Papel do usuário (Admin, Equipe, etc.).</param>
        /// <returns>Token JWT assinado em formato string.</returns>
        string GenerateToken(Guid userId, string name, string email, string role);
    }
}
