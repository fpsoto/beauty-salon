namespace BeautySalon.Application.Common.Interfaces;

// Indirection over DateTime.Now/UtcNow so audit timestamps stay testable and never
// hit the system clock directly from the interceptor.
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
