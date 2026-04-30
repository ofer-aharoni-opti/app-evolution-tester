using Microsoft.AspNetCore.Routing;
using System.Text.RegularExpressions;

namespace Template.WebApi.Routing;

public sealed class KebabCaseParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        var str = value?.ToString();
        return string.IsNullOrEmpty(str) ? null : Regex.Replace(str, "([a-z])([A-Z])", "$1-$2").ToLower();
    }

}