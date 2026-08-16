namespace Sidwell.Core.Domain.Entities;

public sealed class WebauthnCredential
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public byte[] CredentialId { get; set; } = null!;
    public byte[] PublicKey { get; set; } = null!;
    public long SignCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
