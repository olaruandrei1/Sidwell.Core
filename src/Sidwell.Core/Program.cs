using Microsoft.Extensions.Options;
using Prometheus;
using Sidwell.Core.Broadcast;
using Sidwell.Core.Infrastructure;
using Sidwell.Core.Infrastructure.Broadcast;
using Sidwell.Core.Infrastructure.Recalc;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("Sidwell")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:Sidwell in configuration.");

string internalSecret = builder.Configuration["Internal:Secret"]
    ?? builder.Configuration["INTERNAL_SECRET"]
    ?? "sidwell-internal-dev";

builder.Services.AddSidwellInfrastructure(connectionString);

builder.Services.Configure<BroadcastOptions>(builder.Configuration.GetSection(BroadcastOptions.SectionName));
builder.Services.AddHttpClient(BroadcastPublisher.HttpClientName, (sp, c) =>
{
    BroadcastOptions broadcast = sp.GetRequiredService<IOptions<BroadcastOptions>>().Value;
    if (!string.IsNullOrWhiteSpace(broadcast.BaseUrl))
        c.BaseAddress = new Uri(broadcast.BaseUrl);
});
builder.Services.AddScoped<IBroadcastPublisher, BroadcastPublisher>();

WebApplication app = builder.Build();

await app.Services.MigrateAndInstallSidwellSchemaAsync();

app.UseHttpMetrics();
app.Use(async (ctx, next) =>
{
    if (!ctx.Request.Path.StartsWithSegments("/health") && !ctx.Request.Path.StartsWithSegments("/metrics"))
    {
        var start = DateTimeOffset.UtcNow;
        await next();
        app.Logger.LogInformation("{Method} {Path} {Status} {Ms}ms",
            ctx.Request.Method, ctx.Request.Path, ctx.Response.StatusCode,
            (long)(DateTimeOffset.UtcNow - start).TotalMilliseconds);
        return;
    }
    await next();
});

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost("/recalc/{tickerId:guid}", async (
    Guid tickerId, DateOnly? asOf, decimal? technicalScore, IRecalcService recalc, HttpContext http, CancellationToken ct) =>
{
    string? secret = http.Request.Headers["X-Internal-Secret"].FirstOrDefault();
    if (string.IsNullOrEmpty(secret) || secret != internalSecret)
        return Results.StatusCode(403);

    DateOnly date = asOf ?? DateOnly.FromDateTime(DateTime.UtcNow);
    RecalcResult result = await recalc.RecalcTickerAsync(tickerId, date, technicalScore, ct);
    return Results.Ok(result);
});

app.MapMetrics("/metrics");

app.Run();
