using BeautySalon.Application.Common.Interfaces;

namespace BeautySalon.Infrastructure.Services;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
