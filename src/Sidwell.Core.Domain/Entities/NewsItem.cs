namespace Sidwell.Core.Domain.Entities;

public sealed class NewsItem
{
    public Guid Id { get; set; }
    public Guid TickerId { get; set; }
    public string Title { get; set; } = null!;
    public string Url { get; set; } = null!;
    public DateTimeOffset PublishedAt { get; set; }
    public decimal? Sentiment { get; set; }
    public string? Source { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
