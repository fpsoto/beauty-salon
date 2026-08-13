using BeautySalon.Application.Features.Auth;
using BeautySalon.Domain.Entities;
using Xunit;

namespace BeautySalon.Application.Tests.Features.Auth;

public sealed class AuthAppServiceTests : IDisposable
{
    private readonly TestDatabase _db = new();
    private readonly AuthAppService _sut;

    public AuthAppServiceTests()
    {
        _sut = new AuthAppService(_db.UnitOfWork, new FakePasswordHasher(), new LoginRequestValidator());
    }

    public void Dispose() => _db.Dispose();

    private async Task SeedUserAsync(bool isActive = true)
    {
        _db.Context.Add(new User
        {
            Username = "admin",
            PasswordHash = "admin123",
            FullName = "Admin",
            IsActive = isActive
        });
        await _db.Context.SaveChangesAsync();
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_Succeeds()
    {
        await SeedUserAsync();

        var result = await _sut.LoginAsync(new LoginRequest("admin", "admin123"));

        Assert.True(result.IsSuccess);
        Assert.Equal("admin", result.Value.Username);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsInvalidCredentials()
    {
        await SeedUserAsync();

        var result = await _sut.LoginAsync(new LoginRequest("admin", "wrong-password"));

        Assert.True(result.IsFailure);
        Assert.Equal("Auth.InvalidCredentials", result.Error.Code);
    }

    [Fact]
    public async Task LoginAsync_WithUnknownUsername_ReturnsInvalidCredentials()
    {
        var result = await _sut.LoginAsync(new LoginRequest("nobody", "whatever"));

        Assert.True(result.IsFailure);
        Assert.Equal("Auth.InvalidCredentials", result.Error.Code);
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ReturnsInvalidCredentials()
    {
        await SeedUserAsync(isActive: false);

        var result = await _sut.LoginAsync(new LoginRequest("admin", "admin123"));

        Assert.True(result.IsFailure);
        Assert.Equal("Auth.InvalidCredentials", result.Error.Code);
    }

    [Fact]
    public async Task LoginAsync_WithEmptyUsername_ReturnsValidationError()
    {
        var result = await _sut.LoginAsync(new LoginRequest("", "admin123"));

        Assert.True(result.IsFailure);
        Assert.Equal("Auth.InvalidRequest", result.Error.Code);
    }
}
