using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace MarcRocNy.Common.Config;

/// <summary>
/// Edgy behavior, to be avoided.
/// </summary>
public class ConfigEdgyHypotheses
{
    /// <summary>
    /// arrays are kinda goofy. Careful!
    /// </summary>
    public record ArrSetting(ICollection<string?> Arr);

    [Fact]
    public void Array_JsonNullResolvesToEmpty()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes("{ \"arr\" : [\"bill\",\"bob\",null,\"alice\"] }")))
            .Build();

        configuration.Get<ArrSetting>()!.Arr.Should().BeEquivalentTo(["bill", "bob", "", "alice"]);
    }

    [Fact]
    public void Array_IndexedNullIsSkipped()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([new("arr:0", "foo"), new("arr:3", null), new("arr:4", "baz")])
            .Build();

        configuration.Get<ArrSetting>()!.Arr.Should().BeEquivalentTo(["foo", "baz"]);
    }

    [Fact]
    public void Array_IndexesAloneAreOnlyUsedToOrder()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([new("arr:-1", "foo"), new("arr:3", "bar")])
            .Build();

        configuration.Get<ArrSetting>()!.Arr.Should().BeEquivalentTo(["foo", "bar"]);
    }

    [Fact]
    public void Array_IndexesBeforeJson_IndicesWillBeMerged()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([new("arr:1", "fuz"), new("arr:-11", "foo"), new("arr:8", "bar")])
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes("{ \"arr\" : [\"alice\",\"bob\"] }")))
            .Build();

        configuration.Get<ArrSetting>()!.Arr.Should().BeEquivalentTo(["foo", "alice", "bob", "bar"]);
    }

    [Fact]
    public void Array_IndexesAfterJson_IndicesWillBeMerged()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes("{ \"arr\" : [\"alice\",\"bob\"] }")))
            .AddInMemoryCollection([new("arr:1", "fuz"), new("arr:-11", "foo"), new("arr:8", "bar")])
            .Build();

        configuration.Get<ArrSetting>()!.Arr.Should().BeEquivalentTo(["foo", "alice", "fuz", "bar"]);
    }

    [Fact]
    public void Array_IndexesAfterJson_NullWillClearAndSkipJsonEntry()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes("{ \"arr\" : [\"alice\",\"bob\"] }")))
            .AddInMemoryCollection([new("arr:1", null), new("arr:-11", "foo"), new("arr:8", "bar")])
            .Build();

        configuration.Get<ArrSetting>()!.Arr.Should().BeEquivalentTo(["foo", "alice", "bar"]);
    }
}
