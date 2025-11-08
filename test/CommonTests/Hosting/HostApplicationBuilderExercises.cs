using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace MarcRocNy.Common.Hosting;

/// <summary>
/// This test class is just a quick demo of how to use <see cref="HostApplicationBuilder"/> to its greatest effect.
/// </summary>
/// <remarks>
/// I thought of adding some <see cref="IHostBuilder"/> examples here, but that seems so passé. I do miss it though.
/// </remarks>
public class HostApplicationBuilderExercises
{
    /// <summary>
    /// what's "host configuration"?
    /// </summary>
    /// <remarks>
    /// This is something that I don't like about the default experience: it just crushes host and app config together 
    /// </remarks>
    [Fact]
    public void GenericCreateApplicationBuilder_HasFullDefaultConfig()
    {
        string[] args = ["testArg=foobar"];
        IHostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        // this contains all the default config, host and application
        builder.Configuration["testArg"].Should().Be("foobar");
        builder.Configuration.Sources.Should().HaveCount(8);
        builder.Configuration.Sources.Count(s => s.GetType() == typeof(Microsoft.Extensions.Configuration.Memory.MemoryConfigurationSource)).Should().Be(2);
        builder.Configuration.Sources.Count(s => s.GetType() == typeof(Microsoft.Extensions.Configuration.EnvironmentVariables.EnvironmentVariablesConfigurationSource)).Should().Be(2);
        builder.Configuration.Sources.Count(s => s.GetType() == typeof(Microsoft.Extensions.Configuration.CommandLine.CommandLineConfigurationSource)).Should().Be(2);
        builder.Configuration.Sources.Count(s => s.GetType() == typeof(Microsoft.Extensions.Configuration.Json.JsonConfigurationSource)).Should().Be(2);
    }

    [Fact]
    public void HostApplicationBuilderSettings_FromScratch_Empty()
    {
        // without disabling defaults, all the default config will flow into the builder.
        HostApplicationBuilderSettings settings = new() { DisableDefaults = true };

        var builder = Host.CreateApplicationBuilder(settings);
        // it has a memory source, but empty
        builder.Configuration.Sources.Should().HaveCount(1);
        builder.Configuration.Sources.Should().ContainSingle(s => s.GetType() == typeof(Microsoft.Extensions.Configuration.Memory.MemoryConfigurationSource));
        builder.Configuration.AsEnumerable().Should().BeEmpty();
    }

    [Fact]
    public void HostApplicationBuilderSettings_FromScratch_Args()
    {
        // just a quick demo-in-demo of the ways that args are parsed by CommandLineConfigurationSource.
        // which brings us to the best-kept-secret of IConfiguration: it is quite handy as a command-line
        // parsing tool in its own right, and obviates the need for other command-line parsing libraries.
        string[] args_cfg =
        [
            "other=snafu",
            "/testarg", "foobar",
            "--double", "dash",
            "two:level=upstairs",
        ];
        HostApplicationBuilderSettings settings = new()
        {
            Args = args_cfg,
            DisableDefaults = true,
        };

        // adding args gets us args config
        var builder = Host.CreateApplicationBuilder(settings);
        builder.Configuration.Sources.Should().HaveCount(2);
        builder.Configuration.Sources.Should().ContainSingle(s => s.GetType() == typeof(Microsoft.Extensions.Configuration.Memory.MemoryConfigurationSource));
        builder.Configuration.Sources.Should().ContainSingle(s => s.GetType() == typeof(Microsoft.Extensions.Configuration.CommandLine.CommandLineConfigurationSource));
        builder.Configuration["testArg"].Should().Be("foobar");
        builder.Configuration["other"].Should().Be("snafu");
        builder.Configuration["double"].Should().Be("dash");
        builder.Configuration["two:level"].Should().Be("upstairs");
    }

    [Fact]
    public void HostApplicationBuilderSettings_FromScratch_TwoArgs()
    {
        string[] args_prop = ["testArg=foobar"];
        string[] args_cfg = ["other=snafu"];
        HostApplicationBuilderSettings settings = new()
        {
            Args = args_prop,
            DisableDefaults = true,
            Configuration = new ConfigurationManager(),
        };

        // this is kinda silly, but just demonstrates the point that they aren't grouped into one.
        settings.Configuration.AddCommandLine(args_cfg);

        var builder = Host.CreateApplicationBuilder(settings);
        builder.Configuration.Sources.Should().ContainSingle(s => s.GetType() == typeof(Microsoft.Extensions.Configuration.Memory.MemoryConfigurationSource));
        builder.Configuration.Sources.Count(s => s.GetType() == typeof(Microsoft.Extensions.Configuration.CommandLine.CommandLineConfigurationSource)).Should().Be(2);
        builder.Configuration["testArg"].Should().Be("foobar");
        builder.Configuration["other"].Should().Be("snafu");
    }

    [Fact]
    public void HostApplicationBuilderSettings_FromScratch_ClassicHostConfig()
    {
        string[] args = ["testArg=foobar"];
        HostApplicationBuilderSettings settings = new()
        {
            Args = args,
            Configuration = new ConfigurationManager(),
            DisableDefaults = true,
        };

        settings.Configuration.AddEnvironmentVariables("DOTNET_");

        HostApplicationBuilder builder = new(settings);
        builder.Configuration.Sources.Select(s => s.GetType()).Should().BeEquivalentTo([

            typeof(Microsoft.Extensions.Configuration.Memory.MemoryConfigurationSource),
            typeof(Microsoft.Extensions.Configuration.EnvironmentVariables.EnvironmentVariablesConfigurationSource),
            typeof(Microsoft.Extensions.Configuration.CommandLine.CommandLineConfigurationSource),
            ], opt => opt.WithStrictOrdering());

        // this would be dependent on local setup, but default is production
        // builder.Environment.IsDevelopment().Should().BeFalse();
        // builder.Environment.IsProduction().Should().BeTrue();

        // now, let's layer on a conflicting "application" config
        builder.Configuration.AddInMemoryCollection([new("environment", "development")]);

        // nope, still default -- "environment" config is respected.
        builder.Environment.IsDevelopment().Should().BeFalse();
        builder.Environment.IsProduction().Should().BeTrue();
    }

    record RootConfig(string Environment) { public RootConfig() : this("") { } }

    [Fact]
    public void HostApplicationBuilderSettings_FromScratch_SettingsConfigSufficesAsHostConfig()
    {
        string[] args = ["testArg=foobar"];
        HostApplicationBuilderSettings settings = new()
        {
            Args = args,
            Configuration = new ConfigurationManager(),
            DisableDefaults = true,
        };

        // let's simulate an environment config 
        settings.Configuration.AddInMemoryCollection([new("environment", "staging")]);

        HostApplicationBuilder builder = new(settings);
        builder.Configuration.Sources.Select(s => s.GetType()).Should().BeEquivalentTo([
            typeof(Microsoft.Extensions.Configuration.Memory.MemoryConfigurationSource),
            typeof(Microsoft.Extensions.Configuration.Memory.MemoryConfigurationSource),
            typeof(Microsoft.Extensions.Configuration.CommandLine.CommandLineConfigurationSource),
            ], opt => opt.WithStrictOrdering());

        // this would be dependent on local setup, but default is production
        builder.Environment.IsDevelopment().Should().BeFalse();
        builder.Environment.IsStaging().Should().BeTrue();

        // now, let's layer on a conflicting "application" config
        builder.Configuration.AddInMemoryCollection([new("environment", "development")]);

        // nope, still default -- the host "environment" config is respected.
        builder.Environment.IsDevelopment().Should().BeFalse();
        builder.Environment.IsStaging().Should().BeTrue();

        // but the configuration is overridden!
        builder.Configuration["environment"].Should().Be("development");
        builder.Environment.EnvironmentName.Should().Be("staging");

        // this distinction also flows through to the IHost
        builder.Services.Configure<RootConfig>(builder.Configuration);
        IHost host = builder.Build();
        host.Services.GetRequiredService<IHostEnvironment>().IsStaging().Should().BeTrue();
        host.Services.GetRequiredService<IOptions<RootConfig>>().Value.Environment.Should().Be("development");
    }
}
