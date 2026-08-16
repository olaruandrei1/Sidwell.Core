namespace Sidwell.Core.Domain.Entities;

public sealed class UserSetting
{
    public Guid UserId { get; set; }
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
    public DateTimeOffset UpdatedAt { get; set; }
}
