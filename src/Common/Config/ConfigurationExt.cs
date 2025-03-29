using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MarcRocNy.Common.Config;

/// <summary>
/// This establishes supplemental methods within the new pattern.
/// </summary>
public static class ConfigurationExt
{
    /// <summary>
    /// <see cref="OptionsConfigurationServiceCollectionExtensions.Configure{TOptions}(IServiceCollection, IConfiguration)"/>, 
    /// but from the root, with the section-name (or path) defined on the config
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection Configure<TOptions>(this IServiceCollection services, IConfigurationRoot configuration)
        where TOptions : class, ISettings
        => services.Configure<TOptions>(configuration.GetSection(TOptions.SectionName));


}
