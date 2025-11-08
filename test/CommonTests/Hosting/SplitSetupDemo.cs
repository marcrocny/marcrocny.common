using System.Security.Policy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace MarcRocNy.Common.Hosting;

public class SplitSetupDemo
{
}

/// <summary>
/// Example enterprise hosting primitives.
/// </summary>
/// <remarks>
/// Within a given enterprise architecture there will be multiple environments, but there should be a common
/// pattern for configuration through all the environments. It's good to rely on concepts like conventional
/// config and centralized config to effect this. However, this isn't something you can write a library around.
/// At best, you can provide examples, because there are many ways to do this.
/// <para/>
/// In a certain sense, MS hosting primitives _are_ the library. They are beautifully flexible but unfortunately
/// the hosting situation too often calls for a clear, simple, straightforward, opinionated solution internal to
/// the org.
/// <para/>
/// This is one variation that can be used. Others will be added alongside.
/// <para/>
/// You'll see that this builds on what we learned in <see cref="HostApplicationBuilderExercises"/>.
/// </remarks>
public static class MyHostSplit
{
    public const string MyEnvironmenVariablePrefix = "MY_";

    /// <summary>
    /// A bespoke <see cref="HostApplicationBuilder"/> that contains just the host config.
    /// </summary>
    /// <returns></returns>
    public static HostApplicationBuilder CreateApplicationBuilder(string[] args)
    {
        // Start off as close to default as possible.
        // You could override the root path, app name or environment here. That... would probably be silly, but
        // exceptional cases arise. Better that the flexibility exists. Just don't use it.
        HostApplicationBuilderSettings settings = new()
        {
            Args = args,
            Configuration = new ConfigurationManager(),
            DisableDefaults = true,
        };

        // This isn't necessary, but not providing it might be... surprising.
        settings.Configuration.AddEnvironmentVariables("DOTNET_");

        // This can be handy, too, but not necessary if DOTNET_ covers your needs.
        settings.Configuration.AddEnvironmentVariables(MyEnvironmenVariablePrefix);


        return Host.CreateApplicationBuilder(settings);
    }

    /// <summary>
    /// Simulates the distinction between host and application config. Allows interaction with the host config before
    /// application config is added.
    /// </summary>
    /// <remarks>
    /// This could be combined with the above, but I find this aids testing in some nice ways.
    /// </remarks>
    public static HostApplicationBuilder AddMyApplicationConfiguration(this HostApplicationBuilder builder, string[] args)
    {
        // TODO: everything
        return builder;
    }
}
