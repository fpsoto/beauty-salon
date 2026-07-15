namespace BeautySalon.Application.Common.Exceptions;

// Thrown by IUnitOfWork.SaveChangesAsync when the Version optimistic-concurrency
// token no longer matches - translated from the provider's own concurrency exception
// at the Persistence boundary so no EF Core/Sqlite type ever reaches Application.
public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
