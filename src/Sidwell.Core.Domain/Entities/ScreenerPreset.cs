namespace Sidwell.Core.Domain.Entities;

public sealed class ScreenerPreset
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Criteria { get; set; } = "{}";
    public DateTimeOffset CreatedAt { get; set; }
}
