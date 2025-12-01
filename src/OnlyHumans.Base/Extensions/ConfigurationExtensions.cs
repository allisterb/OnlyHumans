namespace OnlyHumans;

using Microsoft.Extensions.Configuration;

public static class ConfigurationExtensions
{
    public static string GetRequiredValue(this IConfigurationRoot configuration, string key)
        => string.IsNullOrEmpty(configuration[key]) ? throw new ArgumentException($"Configuration value {key} is required but not found or empty.") : configuration[key]!;
}
