using System.Threading;
using System.Threading.Tasks;

namespace Evento.Ti.Application.Auth
{
    public interface IAuthService
    {
        Task<LoginResultDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    }
}

