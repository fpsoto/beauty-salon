using BeautySalon.Domain.Common;

namespace BeautySalon.Application.Features.Clients;

public interface IClientAppService
{
    Task<Result<IReadOnlyList<ClientDto>>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ClientDto>>> GetFavoritesAsync(CancellationToken cancellationToken = default);
    Task<Result<ClientDetailDto>> GetDetailAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<Result<ClientDto>> CreateAsync(CreateClientRequest request, CancellationToken cancellationToken = default);
    Task<Result<ClientDto>> UpdateAsync(Guid clientId, UpdateClientRequest request, CancellationToken cancellationToken = default);
    Task<Result> SetFavoriteAsync(Guid clientId, bool isFavorite, CancellationToken cancellationToken = default);
    Task<Result> SetActiveAsync(Guid clientId, bool isActive, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid clientId, CancellationToken cancellationToken = default);
}
