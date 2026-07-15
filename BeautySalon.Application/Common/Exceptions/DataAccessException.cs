namespace BeautySalon.Application.Common.Exceptions;

// Wraps a genuinely unexpected persistence failure (locked db, disk full, corrupt
// file) - these are not business errors, so they stay exceptions (per the Result
// pattern decision) but never leak an EF Core/Sqlite type past Persistence.
public sealed class DataAccessException : Exception
{
    public DataAccessException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
