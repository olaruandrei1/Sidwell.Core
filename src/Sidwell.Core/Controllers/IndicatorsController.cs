using Microsoft.AspNetCore.Mvc;
using Sidwell.Core.Infrastructure.Indicators;

namespace Sidwell.Core.Controllers;

[ApiController]
[Route("indicators")]
public sealed class IndicatorsController(IIndicatorService indicators, InternalSecretOptions internalSecret) : ControllerBase
{
    [HttpGet("{tickerId:guid}")]
    public async Task<IActionResult> GetIndicators(Guid tickerId, [FromQuery] string types, CancellationToken ct)
    {
        string? secret = Request.Headers["X-Internal-Secret"].FirstOrDefault();
        if (string.IsNullOrEmpty(secret) || secret != internalSecret.Secret)
            return StatusCode(403);

        string[] requested = (types ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (requested.Length == 0)
            return BadRequest(new { error = "Query param 'types' is required (comma-separated, e.g. sma20,ema50,rsi14)." });

        IReadOnlyList<IndicatorSeries> result = await indicators.ComputeAsync(tickerId, requested, ct);
        return Ok(result);
    }
}
