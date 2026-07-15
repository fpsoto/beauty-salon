using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Domain.Common;
using FluentValidation;

namespace BeautySalon.Application.Features.Auth;

public sealed class AuthAppService : IAuthAppService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<LoginRequest> _validator;

    public AuthAppService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IValidator<LoginRequest> validator)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _validator = validator;
    }

    public async Task<Result<LoginResultDto>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure<LoginResultDto>(Error.Validation("Auth.InvalidRequest", validation.ToString(" ")));

        var user = await _unitOfWork.Users.GetByUsernameAsync(request.Username, cancellationToken);
        if (user is null || !user.IsActive || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result.Failure<LoginResultDto>(Error.Validation("Auth.InvalidCredentials", "Usuario o contraseña incorrectos."));

        return Result.Success(new LoginResultDto(user.Id, user.Username, user.FullName, user.Role));
    }
}
