using Template.Application.Abstractions;
using Template.Domain.Models;

namespace Template.Infrastructure.MultiTenancy;

public sealed class TenantApplicationContext : ITenantApplicationContext
{
    public TenantContext? Context { get; private set; }

    public void Initialize(int tenantId, string? userName)
    {
        if (tenantId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tenantId), "Tenant ID must be greater than zero.");
        }

        Context = new TenantContext(tenantId, userName);
    }
}
