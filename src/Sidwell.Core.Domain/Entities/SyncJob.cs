namespace Sidwell.Core.Domain.Entities;

public sealed class SyncJob
{
    public Guid Id { get; set; }
    public string Source { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public string? Error { get; set; }
}
