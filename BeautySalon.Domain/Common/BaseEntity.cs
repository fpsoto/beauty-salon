namespace BeautySalon.Domain.Common;

// Guid v7 keeps ids sortable by creation time and collision-free across future
// multi-device/cloud sync, unlike int identity or random Guid v4.
public abstract class BaseEntity
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
}
