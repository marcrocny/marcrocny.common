using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MarcRocNy.Common.Config;

/// <summary>
/// Supplemental methods that enable and establish the <see cref="ISettings.SectionName"/> pattern, as well as normalizing the
/// empty-config behavior to consistently fall back to a config object with sensible defaults.
/// </summary>
public static class ConfigurationExt
{
    /// <summary>
    /// <see cref="OptionsConfigurationServiceCollectionExtensions.Configure{TOptions}(IServiceCollection, IConfiguration)"/>, 
    /// but from the root, with the section-name (or path) pulled from <see cref="ISettings.SectionName"/>.
    /// </summary>
    public static IServiceCollection Configure<TOptions>(this IServiceCollection services, IConfigurationRoot configuration)
        where TOptions : class, ISettings, new()
        => services.Configure<TOptions>(configuration.GetSection<TOptions>());

    /// <summary>
    /// Like, <see cref="ConfigurationBinder.Get{T}(IConfiguration)"/>, but from the root, with the section-name (path) pulled from
    /// <see cref="ISettings.SectionName"/>. Also, never returns `null`.
    /// </summary>
    public static TOptions GetSettings<TOptions>(this IConfigurationRoot configuration)
        where TOptions : class, ISettings, new()
    {
        return configuration.GetSection<TOptions>().Get<TOptions>() ?? new();
    }

    /// <summary>
    /// A combination of <see cref="ConfigureSettings{TOptions}(IServiceCollection, IConfigurationRoot)"/> and
    /// <see cref="GetSettings{TOptions}(IConfigurationRoot)"/>
    /// </summary>
    public static TOptions GetAndConfigure<TOptions>(this IServiceCollection services, IConfigurationRoot configuration)
        where TOptions : class, ISettings, new()
    {
        var section = configuration.GetSection<TOptions>();
        services.Configure<TOptions>(section);
        return section.Get<TOptions>() ?? new();
    }

    /// <summary>
    /// Pulls the configuration section for <see cref="ISettings.SectionName"/>.
    /// </summary>
    public static IConfiguration GetSection<TOptions>(this IConfigurationRoot configuration)
        where TOptions : class, ISettings
        => configuration.GetSection(TOptions.SectionName);
}
