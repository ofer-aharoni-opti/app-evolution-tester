using Template.Application.Abstractions;
using Template.Application.Constants;

namespace Template.Infrastructure.MultiTenancy;

public sealed class TenantContextHandler(ITenantApplicationContext tenantContext) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.Context is { } ctx)
        {
            if (!request.Headers.Contains(TenantHeaders.TenantId))
            {
                request.Headers.TryAddWithoutValidation(TenantHeaders.TenantId, ctx.TenantId.ToString());
            }

            if (!string.IsNullOrWhiteSpace(ctx.UserName) && !request.Headers.Contains(TenantHeaders.UserName))
            {
                request.Headers.TryAddWithoutValidation(TenantHeaders.UserName, ctx.UserName);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
