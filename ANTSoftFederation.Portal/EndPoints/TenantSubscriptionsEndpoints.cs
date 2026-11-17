using System.Security.Claims;
using SorteFederation.ANTApplications.Services;

namespace SorteFederation.ANTApplications.EndPoints;

public sealed class TenantSubscriptionsEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group)
    {
        var routes = group.MapGroup("/tenant-subscriptions").WithTags("Tenant subscriptions");

        routes.MapGet("/plans", async (ClaimsPrincipal user, ANTSoftPortalDbContext db, CancellationToken ct) =>
        {
            if (!IsSystemAdmin(user)) return Results.Forbid();
            var plans = await db.SubscriptionPlans.OrderBy(x => x.Name)
                .Select(x => new PlanResponse(x.Id, x.Name, x.Description, x.MaxApplications, x.MaxUsers, x.DurationMonths, x.Price, x.Currency, x.IsAvailable))
                .ToListAsync(ct);
            return Results.Ok(plans);
        });

        routes.MapGet("/available-plans", async (ANTSoftPortalDbContext db, CancellationToken ct) =>
        {
            var plans = await db.SubscriptionPlans.AsNoTracking().Where(x => x.IsAvailable).OrderBy(x => x.Price).ThenBy(x => x.Name)
                .Select(x => new PlanResponse(x.Id, x.Name, x.Description, x.MaxApplications, x.MaxUsers, x.DurationMonths, x.Price, x.Currency, x.IsAvailable))
                .ToListAsync(ct);
            return Results.Ok(plans);
        });

        routes.MapGet("/current", async (ClaimsPrincipal user, ANTSoftPortalDbContext db, CancellationToken ct) =>
        {
            var tenantClaim = user.FindFirstValue("tenant_id") ?? user.FindFirstValue("company_id") ?? user.FindFirstValue("companyId");
            if (!Guid.TryParse(tenantClaim, out var tenantId) || tenantId == Guid.Empty) return Results.Forbid();
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var current = await db.TenantSubscriptions.IgnoreQueryFilters().AsNoTracking()
                .Where(x => x.PortalTenantId == tenantId && x.Status == TenantSubscriptionStatus.Active && x.StartsOn <= today && x.EndsOn >= today)
                .OrderByDescending(x => x.StartsOn)
                .Select(x => new CurrentSubscriptionResponse(x.Id, x.PlanName, x.MaxApplications, x.MaxUsers,
                    x.DurationMonths, x.PlanPrice, x.PlanCurrency, x.StartsOn, x.EndsOn, x.Status.ToString()))
                .FirstOrDefaultAsync(ct);
            return Results.Ok(new { tenantId, subscription = current });
        });

        routes.MapPost("/plans", async (SavePlanRequest request, ClaimsPrincipal user, ANTSoftPortalDbContext db, CancellationToken ct) =>
        {
            if (!IsSystemAdmin(user)) return Results.Forbid();
            if (!ValidPlan(request)) return Results.BadRequest("Provide a name, positive application/user limits, and a 1, 3, or 12 month term.");
            var name = request.Name.Trim();
            if (await db.SubscriptionPlans.AnyAsync(x => x.Name.ToLower() == name.ToLower(), ct)) return Results.Conflict("A plan with this name already exists.");
            var plan = new SubscriptionPlan { Name = name, Description = request.Description?.Trim(), MaxApplications = request.MaxApplications, MaxUsers = request.MaxUsers, DurationMonths = request.DurationMonths, Price = request.Price, Currency = NormalizeCurrency(request.Currency), IsAvailable = request.IsAvailable };
            db.SubscriptionPlans.Add(plan);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/tenant-subscriptions/plans/{plan.Id}", ToResponse(plan));
        });

        routes.MapPut("/plans/{id:guid}", async (Guid id, SavePlanRequest request, ClaimsPrincipal user, ANTSoftPortalDbContext db, CancellationToken ct) =>
        {
            if (!IsSystemAdmin(user)) return Results.Forbid();
            if (!ValidPlan(request)) return Results.BadRequest("Provide a name, positive application/user limits, and a 1, 3, or 12 month term.");
            var plan = await db.SubscriptionPlans.FindAsync([id], ct);
            if (plan is null) return Results.NotFound();
            var name = request.Name.Trim();
            if (await db.SubscriptionPlans.AnyAsync(x => x.Id != id && x.Name.ToLower() == name.ToLower(), ct)) return Results.Conflict("A plan with this name already exists.");
            plan.Name = name; plan.Description = request.Description?.Trim(); plan.MaxApplications = request.MaxApplications; plan.MaxUsers = request.MaxUsers; plan.DurationMonths = request.DurationMonths; plan.Price = request.Price; plan.Currency = NormalizeCurrency(request.Currency); plan.IsAvailable = request.IsAvailable;
            await db.SaveChangesAsync(ct);
            return Results.Ok(ToResponse(plan));
        });

        routes.MapGet("/tenants", async (ClaimsPrincipal user, ANTSoftPortalDbContext db, CancellationToken ct) =>
        {
            if (!IsSystemAdmin(user)) return Results.Forbid();
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var tenants = await db.PortalTenants.AsNoTracking().OrderBy(x => x.Name).Select(tenant => new TenantSubscriptionResponse(
                tenant.Id, tenant.Code, tenant.Name, tenant.IsActive,
                db.TenantSubscriptions.Where(s => s.PortalTenantId == tenant.Id && s.Status == TenantSubscriptionStatus.Active && s.StartsOn <= today && s.EndsOn >= today)
                    .OrderByDescending(s => s.StartsOn).Select(s => (Guid?)s.Id).FirstOrDefault(),
                db.TenantSubscriptions.Where(s => s.PortalTenantId == tenant.Id && s.Status == TenantSubscriptionStatus.Active && s.StartsOn <= today && s.EndsOn >= today)
                    .OrderByDescending(s => s.StartsOn).Select(s => (Guid?)s.Plan.Id).FirstOrDefault(),
                db.TenantSubscriptions.Where(s => s.PortalTenantId == tenant.Id && s.Status == TenantSubscriptionStatus.Active && s.StartsOn <= today && s.EndsOn >= today)
                    .OrderByDescending(s => s.StartsOn).Select(s => s.Plan.Name).FirstOrDefault(),
                db.TenantSubscriptions.Where(s => s.PortalTenantId == tenant.Id && s.Status == TenantSubscriptionStatus.Active && s.StartsOn <= today && s.EndsOn >= today)
                    .OrderByDescending(s => s.StartsOn).Select(s => (DateOnly?)s.StartsOn).FirstOrDefault(),
                db.TenantSubscriptions.Where(s => s.PortalTenantId == tenant.Id && s.Status == TenantSubscriptionStatus.Active && s.EndsOn >= today)
                    .OrderByDescending(s => s.StartsOn).Select(s => (DateOnly?)s.EndsOn).FirstOrDefault()))
                .ToListAsync(ct);
            return Results.Ok(tenants);
        });

        routes.MapPut("/tenants/{tenantId:guid}", async (Guid tenantId, AssignSubscriptionRequest request, ClaimsPrincipal user, ANTSoftPortalDbContext db, IRealtimeEventBus eventBus, CancellationToken ct) =>
        {
            if (!IsSystemAdmin(user)) return Results.Forbid();
            var tenant = await db.PortalTenants.FindAsync([tenantId], ct);
            if (tenant is null) return Results.NotFound("Tenant not found.");
            var plan = await db.SubscriptionPlans.FirstOrDefaultAsync(x => x.Id == request.PlanId && x.IsAvailable, ct);
            if (plan is null) return Results.BadRequest("Select an available subscription plan.");
            var start = request.StartsOn ?? DateOnly.FromDateTime(DateTime.UtcNow);
            if (request.EndsOn is { } end && end < start) return Results.BadRequest("Subscription end date must be on or after its start date.");
            var existing = await db.TenantSubscriptions.Where(x => x.PortalTenantId == tenantId && x.Status == TenantSubscriptionStatus.Active).ToListAsync(ct);
            foreach (var subscription in existing) subscription.Status = TenantSubscriptionStatus.Cancelled;
            var newSubscription = new TenantSubscription
            {
                PortalTenantId = tenantId, SubscriptionPlanId = plan.Id, PlanName = plan.Name,
                MaxApplications = plan.MaxApplications, MaxUsers = plan.MaxUsers, DurationMonths = plan.DurationMonths,
                PlanPrice = plan.Price, PlanCurrency = plan.Currency,
                StartsOn = start, EndsOn = request.EndsOn ?? start.AddMonths(plan.DurationMonths),
                Status = request.Status, Notes = request.Notes?.Trim()
            };
            db.TenantSubscriptions.Add(newSubscription);
            await db.SaveChangesAsync(ct);
            await eventBus.PublishAsync(new RealtimeEvent
            {
                TenantId = tenantId,
                EventName = RabbitMQTopics.TenantSubscriptionChanged,
                UserFacing = false,
                Data = JsonSerializer.SerializeToElement(new
                {
                    tenantId,
                    tenantName = tenant.Name,
                    hasActiveSubscription = newSubscription.Status == TenantSubscriptionStatus.Active,
                    maxApplications = newSubscription.MaxApplications,
                    maxUsers = newSubscription.MaxUsers,
                    startsOn = newSubscription.StartsOn.ToString("yyyy-MM-dd"),
                    endsOn = newSubscription.EndsOn.ToString("yyyy-MM-dd")
                })
            }, ct);
            return Results.Ok(new TenantSubscriptionResponse(tenant.Id, tenant.Code, tenant.Name, tenant.IsActive, newSubscription.Id, plan.Id, plan.Name, newSubscription.StartsOn, newSubscription.EndsOn));
        });

        routes.MapGet("/tenants/{tenantId:guid}/history", async (Guid tenantId, ClaimsPrincipal user, ANTSoftPortalDbContext db, CancellationToken ct) =>
        {
            if (!IsSystemAdmin(user)) return Results.Forbid();
            if (!await db.PortalTenants.AnyAsync(x => x.Id == tenantId, ct)) return Results.NotFound("Tenant not found.");
            var history = await db.TenantSubscriptions.Where(x => x.PortalTenantId == tenantId)
                .OrderByDescending(x => x.StartsOn)
                .Select(x => new SubscriptionHistoryResponse(x.Id, x.PlanName, x.MaxApplications, x.MaxUsers, x.DurationMonths, x.PlanPrice, x.PlanCurrency, x.StartsOn, x.EndsOn, x.Status, x.Notes,
                    db.TenantSubscriptionPayments.Where(p => p.TenantSubscriptionId == x.Id)
                        .OrderByDescending(p => p.CreatedAt)
                        .Select(p => new PaymentResponse(p.Id, p.TenantSubscriptionId, p.Amount, p.Currency, p.Status, p.PaidAt, p.PaymentMethod, p.ProviderReference, p.Notes, p.CreatedAt)).ToList()))
                .ToListAsync(ct);
            return Results.Ok(history);
        });

        routes.MapPost("/{subscriptionId:guid}/payments", async (Guid subscriptionId, SavePaymentRequest request, ClaimsPrincipal user, ANTSoftPortalDbContext db, CancellationToken ct) =>
        {
            if (!IsSystemAdmin(user)) return Results.Forbid();
            if (request.Amount <= 0) return Results.BadRequest("Payment amount must be greater than zero.");
            var subscription = await db.TenantSubscriptions.FirstOrDefaultAsync(x => x.Id == subscriptionId, ct);
            if (subscription is null) return Results.NotFound("Subscription not found.");
            var payment = new TenantSubscriptionPayment
            {
                TenantSubscriptionId = subscriptionId,
                Amount = request.Amount,
                Currency = NormalizeCurrency(request.Currency),
                Status = request.Status,
                PaidAt = request.Status == TenantSubscriptionPaymentStatus.Paid ? request.PaidAt ?? DateTimeOffset.UtcNow : request.PaidAt,
                PaymentMethod = request.PaymentMethod?.Trim(),
                ProviderReference = request.ProviderReference?.Trim(),
                Notes = request.Notes?.Trim()
            };
            db.TenantSubscriptionPayments.Add(payment);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/tenant-subscriptions/{subscriptionId}/payments/{payment.Id}", new PaymentResponse(payment.Id, payment.TenantSubscriptionId, payment.Amount, payment.Currency, payment.Status, payment.PaidAt, payment.PaymentMethod, payment.ProviderReference, payment.Notes, payment.CreatedAt));
        });
    }

    private static bool IsSystemAdmin(ClaimsPrincipal user) => user.FindAll(ClaimTypes.Role).Any(role => RoleNames.IsSuperAdminRole(role.Value));
    private static bool ValidPlan(SavePlanRequest request) => !string.IsNullOrWhiteSpace(request.Name) && request.Name.Trim().Length <= 120 && request.MaxApplications > 0 && request.MaxUsers > 0 && request.DurationMonths is 1 or 3 or 12 && request.Price is null or >= 0;
    private static string NormalizeCurrency(string? currency) => string.IsNullOrWhiteSpace(currency) ? "USD" : currency.Trim().ToUpperInvariant()[..Math.Min(3, currency.Trim().Length)];
    private static PlanResponse ToResponse(SubscriptionPlan plan) => new(plan.Id, plan.Name, plan.Description, plan.MaxApplications, plan.MaxUsers, plan.DurationMonths, plan.Price, plan.Currency, plan.IsAvailable);

    public sealed record SavePlanRequest(string Name, string? Description, int MaxApplications, int MaxUsers, int DurationMonths, decimal? Price, string? Currency, bool IsAvailable = true);
    public sealed record AssignSubscriptionRequest(Guid PlanId, DateOnly? StartsOn, DateOnly? EndsOn, TenantSubscriptionStatus Status = TenantSubscriptionStatus.Active, string? Notes = null);
    public sealed record SavePaymentRequest(decimal Amount, string? Currency, TenantSubscriptionPaymentStatus Status, DateTimeOffset? PaidAt, string? PaymentMethod, string? ProviderReference, string? Notes);
    public sealed record PlanResponse(Guid Id, string Name, string? Description, int MaxApplications, int MaxUsers, int DurationMonths, decimal? Price, string Currency, bool IsAvailable);
    public sealed record CurrentSubscriptionResponse(Guid Id, string PlanName, int MaxApplications, int MaxUsers, int DurationMonths, decimal? Price, string Currency, DateOnly StartsOn, DateOnly EndsOn, string Status);
    public sealed record TenantSubscriptionResponse(Guid TenantId, string Code, string Name, bool IsTenantActive, Guid? SubscriptionId, Guid? PlanId, string? PlanName, DateOnly? StartsOn, DateOnly? EndsOn);
    public sealed record SubscriptionHistoryResponse(Guid Id, string PlanName, int MaxApplications, int MaxUsers, int DurationMonths, decimal? PlanPrice, string PlanCurrency, DateOnly StartsOn, DateOnly EndsOn, TenantSubscriptionStatus Status, string? Notes, List<PaymentResponse> Payments);
    public sealed record PaymentResponse(Guid Id, Guid SubscriptionId, decimal Amount, string Currency, TenantSubscriptionPaymentStatus Status, DateTimeOffset? PaidAt, string? PaymentMethod, string? ProviderReference, string? Notes, DateTimeOffset CreatedAt);
}
