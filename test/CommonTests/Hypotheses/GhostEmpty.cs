using System;
using System.Collections;
using System.Collections.Generic;

namespace MarcRocNy.Common.Hypotheses;

/// <summary>
/// How I always thought <see cref="Enumerable.Empty{TResult}"/> was implemented...
/// </summary>
internal static class Ghost
{
    public static IEnumerable<T> Empty<T>() => GhostEmpty<T>.Instance;

    private class GhostEmpty<T> : IEnumerable<T>
    {
        public static IEnumerable<T> Instance = new GhostEmpty<T>();

        public IEnumerator GetEnumerator() => GhostEnumerator<T>.Instance;

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GhostEnumerator<T>.Instance;

        private class GhostEnumerator<TInner> : IEnumerator<TInner>
        {
            public static GhostEnumerator<TInner> Instance = new();
            public TInner Current => throw new InvalidOperationException();

            object IEnumerator.Current => throw new InvalidOperationException();

            public void Dispose() { }

            /// <summary>
            /// Where the magic happens: just return `false` on any call to `MoveNext()`.
            /// </summary>
            /// <returns>`false`, every time.</returns>
            public bool MoveNext() => false;

            public void Reset() { }
        }
    }
}