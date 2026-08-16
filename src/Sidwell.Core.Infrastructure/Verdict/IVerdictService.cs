namespace Sidwell.Core.Infrastructure.Verdict;

public sealed record ReentryEstimateResult(int EstimatedDays, int SampleCount, double TargetPrice, double CurrentDeviationPct);

public sealed record TechnicalVerdictResult(
    double RawScore,
    double ConvictionPct,
    string Action,
    double AgreementPct,
    ReentryEstimateResult? Reentry = null);

public interface IVerdictService
{
    Task<TechnicalVerdictResult> ComputeAsync(
        Guid tickerId, double compositeScore, IReadOnlyList<string> types, CancellationToken ct = default);
}
