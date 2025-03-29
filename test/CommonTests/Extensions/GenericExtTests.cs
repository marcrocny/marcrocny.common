using FluentAssertions;
using Xunit;

namespace MarcRocNy.Common.Extensions;

public class GenericExtTests
{
    [Theory]
    [InlineData("valid", "valid")]
    [InlineData(null,null)]
    [InlineData("invalid", null)]
    public void OrNullIf_Cases(string? value, string? expected)
    {
        value.OrNullIf(s => s == "invalid").Should().Be(expected);
    }
}
