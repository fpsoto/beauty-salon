using BeautySalon.Application.Features.Settings;
using BeautySalon.Domain.Entities;
using BeautySalon.Domain.Enums;
using Xunit;

namespace BeautySalon.Application.Tests.Features.Settings;

public sealed class SettingsAppServiceTests : IDisposable
{
    private readonly TestDatabase _db = new();
    private readonly SettingsAppService _sut;
    private readonly Guid _professionalId = Guid.NewGuid();

    public SettingsAppServiceTests()
    {
        _sut = new SettingsAppService(
            _db.UnitOfWork,
            new UpdateAppSettingsRequestValidator(),
            new UpdateWorkingHoursRequestValidator());
    }

    public void Dispose() => _db.Dispose();

    private async Task SeedAppSettingsAsync()
    {
        _db.Context.Add(new AppSettings());
        await _db.Context.SaveChangesAsync();
    }

    // WorkingHours.ProfessionalId has a real FK to User.Id (see AppointmentAppServiceTests) -
    // a bare Guid.NewGuid() with no backing User row fails with a FOREIGN KEY constraint.
    private async Task SeedProfessionalAsync()
    {
        _db.Context.Add(new User { Id = _professionalId, Username = "test-pro", PasswordHash = "hash", FullName = "Test Professional" });
        await _db.Context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetSettingsAsync_ReturnsSeededDefaults()
    {
        await SeedAppSettingsAsync();

        var result = await _sut.GetSettingsAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal("es", result.Value.Language);
    }

    [Fact]
    public async Task UpdateSettingsAsync_PersistsChanges()
    {
        await SeedAppSettingsAsync();

        var result = await _sut.UpdateSettingsAsync(new UpdateAppSettingsRequest("en", AppTheme.Dark, Currency.USD, "MM/dd/yyyy", "hh:mm tt"));

        Assert.True(result.IsSuccess);
        Assert.Equal("en", result.Value.Language);

        var reloaded = await _sut.GetSettingsAsync();
        Assert.Equal("en", reloaded.Value.Language);
        Assert.Equal(AppTheme.Dark, reloaded.Value.Theme);
    }

    [Fact]
    public async Task UpdateSettingsAsync_WithEmptyLanguage_ReturnsValidationError()
    {
        await SeedAppSettingsAsync();

        var result = await _sut.UpdateSettingsAsync(new UpdateAppSettingsRequest("", AppTheme.System, Currency.CLP, "dd/MM/yyyy", "HH:mm"));

        Assert.True(result.IsFailure);
        Assert.Equal("Settings.Invalid", result.Error.Code);
    }

    [Fact]
    public async Task UpdateNotificationRulesAsync_AddsNewRule()
    {
        await SeedAppSettingsAsync();

        var result = await _sut.UpdateNotificationRulesAsync([new NotificationRuleInput(30, true)]);
        var settings = await _sut.GetSettingsAsync();

        Assert.True(result.IsSuccess);
        Assert.Contains(settings.Value.NotificationRules, r => r.MinutesBefore == 30 && r.IsEnabled);
    }

    [Fact]
    public async Task UpdateNotificationRulesAsync_UpsertsExistingRuleByMinutesBefore()
    {
        await SeedAppSettingsAsync();
        await _sut.UpdateNotificationRulesAsync([new NotificationRuleInput(30, true)]);

        await _sut.UpdateNotificationRulesAsync([new NotificationRuleInput(30, false)]);
        var settings = await _sut.GetSettingsAsync();

        var rule30 = Assert.Single(settings.Value.NotificationRules, r => r.MinutesBefore == 30);
        Assert.False(rule30.IsEnabled);
    }

    [Fact]
    public async Task UpdateWorkingHoursAsync_WithNoExistingRows_CreatesThem()
    {
        await SeedProfessionalAsync();

        var result = await _sut.UpdateWorkingHoursAsync(_professionalId, new UpdateWorkingHoursRequest(
            [new WorkingHoursInput(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(18, 0), true)]));

        var hours = await _sut.GetWorkingHoursAsync(_professionalId);

        Assert.True(result.IsSuccess);
        Assert.Single(hours.Value);
        Assert.Equal(new TimeOnly(18, 0), hours.Value[0].EndTime);
    }

    [Fact]
    public async Task UpdateWorkingHoursAsync_WithExistingRow_UpdatesInPlaceRatherThanDuplicating()
    {
        await SeedProfessionalAsync();
        await _sut.UpdateWorkingHoursAsync(_professionalId, new UpdateWorkingHoursRequest(
            [new WorkingHoursInput(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(18, 0), true)]));

        await _sut.UpdateWorkingHoursAsync(_professionalId, new UpdateWorkingHoursRequest(
            [new WorkingHoursInput(DayOfWeek.Monday, new TimeOnly(10, 0), new TimeOnly(17, 0), true)]));

        var hours = await _sut.GetWorkingHoursAsync(_professionalId);

        Assert.Single(hours.Value);
        Assert.Equal(new TimeOnly(10, 0), hours.Value[0].StartTime);
        Assert.Equal(new TimeOnly(17, 0), hours.Value[0].EndTime);
    }

    [Fact]
    public async Task UpdateWorkingHoursAsync_WithEndBeforeStartOnWorkingDay_ReturnsValidationError()
    {
        var result = await _sut.UpdateWorkingHoursAsync(_professionalId, new UpdateWorkingHoursRequest(
            [new WorkingHoursInput(DayOfWeek.Monday, new TimeOnly(18, 0), new TimeOnly(9, 0), true)]));

        Assert.True(result.IsFailure);
        Assert.Equal("WorkingHours.Invalid", result.Error.Code);
    }
}
