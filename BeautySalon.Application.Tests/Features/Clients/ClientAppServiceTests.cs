using BeautySalon.Application.Features.Clients;
using Xunit;

namespace BeautySalon.Application.Tests.Features.Clients;

public sealed class ClientAppServiceTests : IDisposable
{
    private readonly TestDatabase _db = new();
    private readonly ClientAppService _sut;

    public ClientAppServiceTests()
    {
        _sut = new ClientAppService(
            _db.UnitOfWork,
            new CreateClientRequestValidator(),
            new UpdateClientRequestValidator());
    }

    public void Dispose() => _db.Dispose();

    private static CreateClientRequest ValidCreateRequest(string rut = "12345678-5") =>
        new("Maria", "Gonzalez", rut, "+56911111111", "maria@example.com", null, null, null);

    [Fact]
    public async Task CreateAsync_WithValidData_Succeeds()
    {
        var result = await _sut.CreateAsync(ValidCreateRequest());

        Assert.True(result.IsSuccess);
        Assert.Equal("Maria", result.Value.Name);
        Assert.Equal("12345678-5", result.Value.Rut);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateRut_ReturnsConflict()
    {
        await _sut.CreateAsync(ValidCreateRequest());

        var result = await _sut.CreateAsync(ValidCreateRequest());

        Assert.True(result.IsFailure);
        Assert.Equal("Client.DuplicateRut", result.Error.Code);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidRutFormat_ReturnsValidationError()
    {
        var request = ValidCreateRequest("not-a-rut");

        var result = await _sut.CreateAsync(request);

        Assert.True(result.IsFailure);
        Assert.Equal("Client.Invalid", result.Error.Code);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyName_ReturnsValidationError()
    {
        var request = ValidCreateRequest() with { Name = "" };

        var result = await _sut.CreateAsync(request);

        Assert.True(result.IsFailure);
        Assert.Equal("Client.Invalid", result.Error.Code);
    }

    [Fact]
    public async Task UpdateAsync_KeepingSameRut_Succeeds()
    {
        var created = await _sut.CreateAsync(ValidCreateRequest());

        var updateRequest = new UpdateClientRequest("Maria", "Gonzalez Soto", "12345678-5", "+56922222222", null, null, null, null);
        var result = await _sut.UpdateAsync(created.Value.Id, updateRequest);

        Assert.True(result.IsSuccess);
        Assert.Equal("Gonzalez Soto", result.Value.LastName);
    }

    [Fact]
    public async Task UpdateAsync_ChangingRutToAnotherClientsRut_ReturnsConflict()
    {
        var firstClient = await _sut.CreateAsync(ValidCreateRequest("12345678-5"));
        var secondClient = await _sut.CreateAsync(ValidCreateRequest("11111111-1"));

        var updateRequest = new UpdateClientRequest("Maria", "Gonzalez", "11111111-1", "+56911111111", null, null, null, null);
        var result = await _sut.UpdateAsync(firstClient.Value.Id, updateRequest);

        Assert.True(result.IsFailure);
        Assert.Equal("Client.DuplicateRut", result.Error.Code);
        Assert.NotEqual(firstClient.Value.Id, secondClient.Value.Id);
    }

    [Fact]
    public async Task SetFavoriteAsync_TogglesFavoriteFlag()
    {
        var created = await _sut.CreateAsync(ValidCreateRequest());

        var result = await _sut.SetFavoriteAsync(created.Value.Id, true);
        var detail = await _sut.GetDetailAsync(created.Value.Id);

        Assert.True(result.IsSuccess);
        Assert.True(detail.Value.Client.IsFavorite);
    }

    [Fact]
    public async Task DeleteAsync_ThenSearch_DoesNotReturnClient()
    {
        var created = await _sut.CreateAsync(ValidCreateRequest());

        await _sut.DeleteAsync(created.Value.Id);
        var searchResult = await _sut.SearchAsync("Maria");

        Assert.True(searchResult.IsSuccess);
        Assert.DoesNotContain(searchResult.Value, c => c.Id == created.Value.Id);
    }
}
