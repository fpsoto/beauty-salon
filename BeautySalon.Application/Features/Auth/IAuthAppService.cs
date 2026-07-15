using BeautySalon.Domain.Common;

namespace BeautySalon.Application.Features.Auth;

public interface IAuthAppService
{
    Task<Result<LoginResultDto>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
