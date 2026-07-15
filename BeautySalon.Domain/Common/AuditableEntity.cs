namespace BeautySalon.Domain.Common;

// Every entity carries the full audit trail + soft delete + optimistic concurrency,
// per explicit requirement - applies uniformly, even to simple catalog entities.
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public bool IsDeleted { get; set; }

    // Plain int (not a provider-specific rowversion) so it stays portable across
    // SQLite/SQL Server/PostgreSQL if the persistence provider is swapped later.
    public int Version { get; set; } = 1;
}
