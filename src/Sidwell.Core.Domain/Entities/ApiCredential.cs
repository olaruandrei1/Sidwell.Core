namespace Sidwell.Core.Domain.Entities;

public sealed class ApiCredential
{
    public Guid Id { get; set; }
    public string Provider { get; set; } = null!;
    public string EncryptedKey { get; set; } = null!;
    public DateTimeOffset RotatedAt { get; set; }
}
