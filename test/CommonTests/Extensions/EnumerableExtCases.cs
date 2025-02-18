using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MarcRocNy.Common.Extensions;

internal class EnumerableExtCases
{
    public static IEnumerable<TheoryDataRow<int[]?>> EmptyAndNullInts()
    {
        yield return new(null);
        yield return new([]);
        //yield return new(new List<int>(0));
    }

    public static IEnumerable<TheoryDataRow<string[]>> EnumsWithThings()
    {
        yield return new(["one", "two", "three"]);
        //yield return new(new List<string> { "one", "two", "three" });
        //yield return new(new[] { "one", "two", "three" }.NullIfEmptyToList()!);
    }

    public static IEnumerable<TheoryDataRow<string[], string[], bool>> EnumerableEqualityCases()
    {
        yield return new
        (
            ["uno", "dos", "tres", "catorce"],
            ["uno", "dos", "tres", "catorce"],
            true
        );
        yield return new
        (
            ["uno", "dos", "tres", "catorce", "uno"],
            ["uno", "dos", "tres", "catorce", "tres"],
            true
        );
        yield return new
        (
            ["uno", "dos", "tres", "catorce"],
            ["tres", "dos", "catorce", "uno"],
            true
        );
        yield return new
        (
            ["uno", "dos", "tres", "quatro"],
            ["uno", "dos", "tres", "catorce"],
            false
        );
        yield return new
        (
            ["uno", "dos", "tres", "catorce"],
            ["uno", "dos", "tres", "catorce", "you too"],
            false
        );
    }

    public static IEnumerable<TheoryDataRow<string[], string[]>> EnumerableEqualityCasesBare()
        => EnumerableEqualityCases().Select(c => new TheoryDataRow<string[], string[]>(c.Data.Item1, c.Data.Item2));
}
