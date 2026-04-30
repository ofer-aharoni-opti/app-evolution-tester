using Microsoft.Extensions.Configuration;

namespace Template.Testing.Common.Extensions;

public static class ConfigurationExtensions
{
    private const string EnvironmentVariablesSectionName = "EnvironmentVariables"; 

    public static List<KeyValuePair<string, string?>> GetEnvironmentVariables(this IConfiguration configuration)
    {
        return configuration.GetSection(EnvironmentVariablesSectionName)
            .GetChildren()
            .ToList()
            .Select(x => KeyValuePair.Create(x.Key, x.Value))
            .ToList();
    }
}