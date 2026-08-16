namespace Sidwell.Core.Domain.Entities;

public sealed class TickerNote
{
    public Guid UserId { get; set; }
    public Guid TickerId { get; set; }
    public string Body { get; set; } = null!;
    public DateTimeOffset UpdatedAt { get; set; }
}
