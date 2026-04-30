using Template.Domain.Models;

namespace Template.Application.Abstractions;

public interface ITenantApplicationContext
{
    TenantContext? Context { get; }
    void Initialize(int tenantId, string? userName);
}
