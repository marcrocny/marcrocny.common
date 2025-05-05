using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace MarcRocNy.Common.Config;

public class ConfigurationExtShould
{
    public enum ImplType { Dev, Demo, ProdSvc }

    /// <summary>
    /// Example config with a variety of property types.
    /// </summary>
    public record Settings(
        int Count = 1, //technically covered by `this(1)` below.
        double Level = 0,
        decimal Amount = 0,
        string Connection = "",
        bool Active = false,
        ImplType Impl = ImplType.Dev
        ) : ISettingPointer
    {
        public static string SectionName => "section";

        /// <summary>
        /// Satisfies the `new()` generic constraint, ensures every property has a default
        /// and allows creation from an empty <see cref="IConfiguration"/>.
        /// </summary>
        public Settings() : this(1) { }

        // these are declared directly because record-constructor defaults must be compile-time constants.

        public DateOnly Cutoff { get; init; } = DateOnly.MinValue;
        public TimeSpan WaitThreshold { get; init; } = TimeSpan.Zero;
        public TimeOnly EventStart { get; init; } = TimeOnly.MinValue;
        public Dictionary<int, string> NumMap { get; init; } = [];
        public ICollection<string?> Names { get; init; } = [];
    }

    [Fact]
    public void DeserializeAsExpected()
    {
        DateTime date = DateTime.Now;
        Settings expected = new(
            5,
            3.1415,
            12.34m,
            "fubar",
            true,
            ImplType.Demo
            )
        {
            Cutoff = DateOnly.FromDateTime(date),
            WaitThreshold = TimeSpan.FromSeconds(140),
            EventStart = TimeOnly.FromDateTime(date),
            NumMap = new() { [1] = "one", [4] = "quatro" },
            Names = ["", "alice"],
        };


        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{Settings.SectionName}:{nameof(Settings.Count)}"] = "5",
                [$"{Settings.SectionName}:{nameof(Settings.Level)}"] = "3.1415",
                [$"{Settings.SectionName}:{nameof(Settings.Amount)}"] = "12.34",
                [$"{Settings.SectionName}:{nameof(Settings.Connection)}"] = "fubar",
                [$"{Settings.SectionName}:{nameof(Settings.Cutoff)}"] = date.Date.ToShortDateString(),
                [$"{Settings.SectionName}:{nameof(Settings.WaitThreshold)}"] = "0:02:20",
                [$"{Settings.SectionName}:{nameof(Settings.EventStart)}"] = TimeOnly.FromDateTime(date).ToString("o"),
                [$"{Settings.SectionName}:{nameof(Settings.Active)}"] = "true",
                [$"{Settings.SectionName}:{nameof(Settings.NumMap)}:1"] = "one",
                [$"{Settings.SectionName}:{nameof(Settings.NumMap)}:4"] = "quatro",
                [$"{Settings.SectionName}:{nameof(Settings.Names)}:0"] = "",
                [$"{Settings.SectionName}:{nameof(Settings.Names)}:1"] = "alice",
                [$"{Settings.SectionName}:{nameof(Settings.Impl)}"] = "demo",
            })
            .Build();

        // a-a
        configuration.GetSettings<Settings>().Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ResolveAsExpected()
    {
        DateTime date = DateTime.Now;
        Settings expected = new(
            5,
            1.618,
            12.34m,
            "fubar",
            true,
            ImplType.ProdSvc
            )
        {
            Cutoff = DateOnly.FromDateTime(date),
            WaitThreshold = TimeSpan.FromSeconds(140),
            EventStart = TimeOnly.FromDateTime(date),
            NumMap = new() { [1] = "one", [4] = "quatro" },
            Names = ["", "alice"],
        };


        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{Settings.SectionName}:{nameof(Settings.Count)}"] = "5",
                [$"{Settings.SectionName}:{nameof(Settings.Level)}"] = "1.618",
                [$"{Settings.SectionName}:{nameof(Settings.Amount)}"] = "12.34",
                [$"{Settings.SectionName}:{nameof(Settings.Connection)}"] = "fubar",
                [$"{Settings.SectionName}:{nameof(Settings.Cutoff)}"] = date.Date.ToShortDateString(),
                [$"{Settings.SectionName}:{nameof(Settings.WaitThreshold)}"] = "0:02:20",
                [$"{Settings.SectionName}:{nameof(Settings.EventStart)}"] = TimeOnly.FromDateTime(date).ToString("o"),
                [$"{Settings.SectionName}:{nameof(Settings.Active)}"] = "true",
                [$"{Settings.SectionName}:{nameof(Settings.NumMap)}:1"] = "one",
                [$"{Settings.SectionName}:{nameof(Settings.NumMap)}:4"] = "quatro",
                [$"{Settings.SectionName}:{nameof(Settings.Names)}:0"] = "",
                [$"{Settings.SectionName}:{nameof(Settings.Names)}:1"] = "alice",
                [$"{Settings.SectionName}:{nameof(Settings.Impl)}"] = "2",
            })
            .Build();
        IServiceProvider serviceProvider = new ServiceCollection()
            .Configure<Settings>(configuration)
            .BuildServiceProvider();

        // a-a
        serviceProvider.GetRequiredService<IOptions<Settings>>().Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void BuildFromEmptyAsExpected()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder().Build();
        configuration.GetSettings<Settings>().Should().BeEquivalentTo(new Settings());
    }

    [Fact]
    public void ResolveFromEmptyAsExpected()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder().Build();
        IServiceProvider serviceProvider = new ServiceCollection()
            .Configure<Settings>(configuration)
            .BuildServiceProvider();

        // a-a
        serviceProvider.GetRequiredService<IOptions<Settings>>().Value.Should().BeEquivalentTo(new Settings());
    }
}
