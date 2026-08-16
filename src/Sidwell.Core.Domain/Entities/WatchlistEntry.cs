namespace Sidwell.Core.Domain.Entities;

public sealed class WatchlistEntry
{
    public Guid UserId { get; set; }
    public Guid TickerId { get; set; }
    public DateTimeOffset AddedAt { get; set; }
}
