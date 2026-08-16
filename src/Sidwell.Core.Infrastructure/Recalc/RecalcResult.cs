namespace Sidwell.Core.Infrastructure.Recalc;

public sealed record RecalcResult(
    Guid TickerId,
    DateOnly AsOfDate,
    List<string> Ran,
    List<RecalcSkip> Skipped);

public sealed record RecalcSkip(string Step, string Reason);
