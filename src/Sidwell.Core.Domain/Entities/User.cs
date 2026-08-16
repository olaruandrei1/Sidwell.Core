namespace Sidwell.Core.Domain.Entities;

public sealed class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string? DisplayName { get; set; }
    public bool IsAdmin { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
