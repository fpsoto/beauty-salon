using BeautySalon.Domain.Enums;

namespace BeautySalon.Application.Features.Auth;

public sealed record LoginResultDto(Guid UserId, string Username, string FullName, UserRole Role);
