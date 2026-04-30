namespace Template.WebApi.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AllowWithNoTenantHeader : Attribute;
