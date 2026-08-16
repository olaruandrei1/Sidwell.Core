namespace Sidwell.Core.Domain.Entities;

public sealed class SecFiling
{
    public Guid Id { get; set; }
    public Guid TickerId { get; set; }
    public string FormType { get; set; } = null!;
    public DateOnly FilingDate { get; set; }
    public string AccessionNo { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}
