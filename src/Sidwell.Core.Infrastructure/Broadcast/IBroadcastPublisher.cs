namespace Sidwell.Core.Infrastructure.Broadcast;

public interface IBroadcastPublisher
{
    Task PublishAsync(string eventName, Guid? userId, object payload, CancellationToken ct = default);
}
