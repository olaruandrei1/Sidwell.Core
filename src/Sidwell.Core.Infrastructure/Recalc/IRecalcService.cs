namespace Sidwell.Core.Infrastructure.Recalc;

public interface IRecalcService
{
    Task<RecalcResult> RecalcTickerAsync(Guid tickerId, DateOnly asOf, decimal? technicalScore = null, CancellationToken ct = default);
}
