using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace MarcRocNy.Common.Config;

public class ConfigDefaultsHypotheses
{
    // These two below are rather disappointing. Even with sensible defaults it yields null.
    // Note that if even one NONMATCHING element is present in the configuration (section) it will yield with defaults!!
    public record HasDefaults(int Count = 1, string Name = "foo");

    [Fact]
    public void GetWhenEmptyWithoutParameterless_ReturnsNull()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                //["count"] = "4",
                //["neume"] = "bar",
            })
            .Build();

        configuration.Get<HasDefaults>().Should().BeNull(); //.BeEquivalentTo(new NoParameterless(Name: "bar"));
    }

    public class HasParameterless { public int Count { get; init; } = 1; public string Name { get; init; } = "foo"; }

    [Fact]
    public void GetWhenEmptyWithParameterless_ReturnsNull()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            //.AddInMemoryCollection(new Dictionary<string, string?> 
            //{ 
            //    //["count"] = "4",
            //    ["name"] = "bar",
            //})
            .Build();

        configuration.Get<HasParameterless>().Should().BeNull(); //.BeEquivalentTo(new NoParameterless(Name: "bar"));
    }

    /// <summary>
    /// Without a parameterless constructor, even with defaults, Activator is lost. This is why 
    /// <see cref="ConfigurationExt.ConfigureSettings{TOptions}(IServiceCollection, IConfigurationRoot)"/> includes
    /// a `new()` generic constraint: it heads off a potential run-time error.
    /// </summary>
    [Fact]
    public void ResolveOptionsWhenEmptyWithoutParameterless_Throws()
    {
        IConfiguration configuration = new ConfigurationBuilder().Build();
        IServiceProvider services = new ServiceCollection()
            .Configure<HasDefaults>(configuration)
            .BuildServiceProvider();

        var options = services.GetRequiredService<IOptions<HasDefaults>>();
        options.Should().NotBeNull();

        Func<HasDefaults> act = () => options.Value;
        act.Should().Throw<MissingMethodException>();
    }

    /// <summary>
    /// This works! ... and yet, the difference to <see cref="ConfigurationBinder.Get{T}(IConfiguration)"/> 
    /// (which returns `null`) is a bit queasy.
    /// </summary>
    [Fact]
    public void ResolveWhenEmptyWithParameterless_ReturnsWithObjectDefaults()
    {
        IConfiguration configuration = new ConfigurationBuilder().Build();
        IServiceProvider services = new ServiceCollection()
            .Configure<HasParameterless>(configuration)
            .BuildServiceProvider();

        var options = services.GetRequiredService<IOptions<HasParameterless>>();
        options.Should().NotBeNull();
        options.Value.Should().BeEquivalentTo(new HasParameterless());
    }

    public record NoDefaults(int Count, string Name);

    /// <summary>
    /// This shows how a config record without any defaults is especially unforgiving.
    /// </summary>
    [Fact]
    public void GetWhenEmptyWithoutDefaults_um()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                //["count"] = "4",
                ["name"] = "bar",
            })
            .Build();

        Action act = () => configuration.Get<NoDefaults>();
        act.Should().Throw<InvalidOperationException>();
    }
}
