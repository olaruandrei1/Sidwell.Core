using Microsoft.AspNetCore.Mvc;
using Sidwell.Core.Infrastructure.Verdict;

namespace Sidwell.Core.Controllers;

public sealed record VerdictRequest(double CompositeScore, IReadOnlyList<string> Types);

[ApiController]
[Route("verdict")]
public sealed class VerdictController(IVerdictService verdictService, InternalSecretOptions internalSecret) : ControllerBase
{
    [HttpPost("{tickerId:guid}")]
    public async Task<IActionResult> GetVerdict(Guid tickerId, [FromBody] VerdictRequest request, CancellationToken ct)
    {
        string? secret = Request.Headers["X-Internal-Secret"].FirstOrDefault();
        if (string.IsNullOrEmpty(secret) || secret != internalSecret.Secret)
            return StatusCode(403);

        if (request.Types.Count == 0)
            return BadRequest(new { error = "'types' must contain at least one indicator." });

        TechnicalVerdictResult result = await verdictService.ComputeAsync(tickerId, request.CompositeScore, request.Types, ct);
        return Ok(result);
    }
}
