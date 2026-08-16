namespace Sidwell.Core.Infrastructure.Indicators;

public interface IIndicatorService
{
    Task<IReadOnlyList<IndicatorSeries>> ComputeAsync(Guid tickerId, IReadOnlyList<string> requestedTypes, CancellationToken ct = default);
}
