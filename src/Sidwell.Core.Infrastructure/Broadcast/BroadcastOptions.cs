namespace Sidwell.Core.Infrastructure.Broadcast;

public sealed class BroadcastOptions
{
    public const string SectionName = "Broadcast";

    public string BaseUrl { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
}
