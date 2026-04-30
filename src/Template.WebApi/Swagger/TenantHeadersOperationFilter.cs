using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Template.WebApi.Attributes;
using Template.Application.Constants;

namespace Template.WebApi.Swagger;

public sealed class TenantHeadersOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<IOpenApiParameter>();

        var allowsNoTenantHeader = context.ApiDescription.ActionDescriptor.EndpointMetadata
            .Any(x => x is AllowWithNoTenantHeader);

        var requiresUserNameHeader = context.ApiDescription.ActionDescriptor.EndpointMetadata
            .Any(x => x is RequireUserNameHeader);

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = TenantHeaders.TenantId,
            In = ParameterLocation.Header,
            Required = !allowsNoTenantHeader
        });

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = TenantHeaders.UserName,
            In = ParameterLocation.Header,
            Required = requiresUserNameHeader
        });
    }
}
