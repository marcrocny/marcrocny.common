using System;

namespace MarcRocNy.Common.Extensions;

public static class GenericExt
{
    /// <summary>
    /// Syntax sugar: use mainly to combine (very simple!) validation under a null-coalescing operator.
    /// </summary>
    public static T? OrNullIf<T>(this T? source, Func<T, bool> predicate)
        => source == null ? source : predicate(source) ? default : source;
}
