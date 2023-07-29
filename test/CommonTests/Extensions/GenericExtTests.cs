using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MarcRocNy.Common.Extensions
{
    public class GenericExtTests
    {
        [Theory]
        [InlineData("valid", "valid")]
        [InlineData(null,null)]
        [InlineData("invalid", null)]
        public void OrNullIf_Cases(string value, string expected)
        {
            value.OrNullIf(s => s == "invalid").Should().Be(expected);
        }
    }
}
