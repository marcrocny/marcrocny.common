using System.Collections.Generic;
using System.Linq;

namespace MarcRocNy.Common.Extensions;

internal class EnumerableExtCases
{
    public static IEnumerable<object[]> EmptyAndNullInts()
    {
        yield return new object[] { null };
        yield return new object[] { Enumerable.Empty<int>() };
        yield return new object[] { new List<int>(0) };
    }

    public static IEnumerable<object[]> EnumsWithThings()
    {
        yield return new object[] { new[] { "one", "two", "three" } };
        yield return new object[] { new List<string> { "one", "two", "three" } };
        yield return new object[] { new[] { "one", "two", "three" }.NullIfEmptyToList()! };
    }

    public static IEnumerable<object[]> EnumerableEqualityCases()
    {
        yield return new object[]
        {
            new[] {"uno", "dos", "tres", "catorce"},
            new[] {"uno", "dos", "tres", "catorce"},
            true
        };
        yield return new object[]
        {
            new[] {"uno", "dos", "tres", "catorce", "uno"},
            new[] {"uno", "dos", "tres", "catorce", "tres"},
            true
        };
        yield return new object[]
        {
            new[] {"uno", "dos", "tres", "catorce"},
            new[] { "tres", "dos", "catorce", "uno"},
            true
        };
        yield return new object[]
        {
            new[] {"uno", "dos", "tres", "quatro"},
            new[] {"uno", "dos", "tres", "catorce"},
            false
        };
        yield return new object[]
        {
            new[] {"uno", "dos", "tres", "catorce"},
            new[] {"uno", "dos", "tres", "catorce", "you too"},
            false
        };
    }



}
