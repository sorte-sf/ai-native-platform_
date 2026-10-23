using System.Security.Claims;
using SorteFederation.Builders.Data.Domain;
using Microsoft.EntityFrameworkCore;

namespace SorteFederation.Builders.Services;

public sealed record AiUsageReservation(Guid TenantId, DateOnly PeriodStart, long ReservedTokens);

public sealed class AiQuotaExceededException(string message) : InvalidOperationException(message);

public sealed class AiUsageMeter(BuilderDbContext db, IHttpContextAccessor accessor)
{
    public async Task<AiUsageReservation> ReserveAsync(ResolvedAiPolicy policy, long inputEstimate, CancellationToken token)
    {
        var periodStart = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var reserved = checked(Math.Max(0, inputEstimate) + policy.MaxOutputTokens);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO builders."AiUsagePeriods" ("Id", "TenantId", "PeriodStart", "UsedTokens", "Requests", "ReservedTokens", "ReservedRequests", "UpdatedAt")
            VALUES ({Guid.CreateVersion7()}, {policy.TenantId}, {periodStart}, 0, 0, 0, 0, {DateTimeOffset.UtcNow})
            ON CONFLICT ("TenantId", "PeriodStart") DO NOTHING
            """, token);

        var updated = await db.AiUsagePeriods.Where(x => x.TenantId == policy.TenantId && x.PeriodStart == periodStart)
            .Where(x => policy.MonthlyTokenLimit == 0 || x.UsedTokens + x.ReservedTokens + reserved <= policy.MonthlyTokenLimit)
            .Where(x => policy.MonthlyRequestLimit == 0 || x.Requests + x.ReservedRequests + 1 <= policy.MonthlyRequestLimit)
            .ExecuteUpdateAsync(update => update
                .SetProperty(x => x.ReservedTokens, x => x.ReservedTokens + reserved)
                .SetProperty(x => x.ReservedRequests, x => x.ReservedRequests + 1)
                .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow), token);
        if (updated != 1) throw new AiQuotaExceededException("This tenant has reached its AI usage limit for the current month.");
        return new AiUsageReservation(policy.TenantId, periodStart, reserved);
    }

    public async Task RecordAsync(ResolvedAiPolicy policy, AiUsageReservation? reservation, Guid? applicationId,
        string feature, string provider, string model, long inputTokens, long outputTokens, bool estimated, string outcome,
        string? failureCode, long durationMilliseconds, CancellationToken token)
    {
        var normalizedInput = Math.Max(0, inputTokens);
        var normalizedOutput = Math.Max(0, outputTokens);
        var usedTokens = checked(normalizedInput + normalizedOutput);
        var requestId = Guid.CreateVersion7();
        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Retries must not retain entities from a failed/rolled-back attempt.
            db.ChangeTracker.Clear();

            // A connection can fail while CommitAsync is returning even though the
            // transaction reached PostgreSQL. This stable request id makes retries
            // idempotent and prevents the quota reservation being charged twice.
            if (await db.AiUsage.AsNoTracking().AnyAsync(x => x.RequestId == requestId, token)) return;

            await using var transaction = await db.Database.BeginTransactionAsync(token);
            if (reservation is not null)
            {
                var updated = await db.AiUsagePeriods.Where(x => x.TenantId == reservation.TenantId && x.PeriodStart == reservation.PeriodStart &&
                        x.ReservedTokens >= reservation.ReservedTokens && x.ReservedRequests > 0)
                    .ExecuteUpdateAsync(update => update
                        .SetProperty(x => x.ReservedTokens, x => x.ReservedTokens - reservation.ReservedTokens)
                        .SetProperty(x => x.ReservedRequests, x => x.ReservedRequests - 1)
                        .SetProperty(x => x.UsedTokens, x => x.UsedTokens + usedTokens)
                        .SetProperty(x => x.Requests, x => x.Requests + 1)
                        .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow), token);
                if (updated != 1) throw new InvalidOperationException("AI usage reservation could not be settled.");
            }

            var userId = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? accessor.HttpContext?.User.FindFirstValue("sub");
            db.AiUsage.Add(new BuilderAiUsage
            {
                TenantId = policy.TenantId,
                ApplicationId = applicationId,
                UserId = userId,
                Feature = feature,
                Provider = provider,
                Model = model,
                InputTokens = normalizedInput,
                OutputTokens = normalizedOutput,
                IsEstimated = estimated,
                Outcome = outcome,
                FailureCode = failureCode,
                DurationMilliseconds = Math.Max(0, durationMilliseconds),
                RequestId = requestId
            });
            await db.SaveChangesAsync(token);
            await transaction.CommitAsync(token);
        });
    }

    public static long EstimateTokens(string text) => (long)Math.Ceiling(text.Length / 3.5d);
}
