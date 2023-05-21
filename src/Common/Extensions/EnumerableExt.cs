using System;
using System.Collections.Generic;
using System.Linq;

namespace MarcRocNy.Common.Extensions
{
    /// <summary>
    /// Extensions over <see cref="IEnumerable{T}"/> and other related types.
    /// </summary>
    public static class EnumerableExt
    {
        /// <summary>
        /// Substitutes <see cref="Enumerable.Empty{TResult}"/> for a null enumerable.
        /// </summary>
        /// <remarks>
        /// Enumerables are empty, not null. Still, enumerables consumed from an external source may be null. 
        /// This should be before any consumption. Nullable references make this easier to enforce.
        /// </remarks>
        public static IEnumerable<T> Safe<T>(this IEnumerable<T>? source) => source ?? Enumerable.Empty<T>();

        /// <summary>
        /// <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})"/>, but preserves null.
        /// </summary>
        public static List<T>? Clone<T>(this IEnumerable<T>? listToClone) => listToClone?.ToList();

        /// <summary>
        /// Iterates source using a single test-transformation function, and yields transformed elements where the test returns `true`.
        /// </summary>
        /// <remarks>
        /// This is handy for transforming using the pre-tuple `bool TryXxx(..., out )` pattern.
        /// e.g.: `IEnumerable<int> ints = strings.TrySelect(s => (int.TryParse(s, out var i), i));`
        /// </remarks>
        public static IEnumerable<TResult> TrySelect<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, (bool, TResult)> conditionalSelector)
        {
            foreach(TSource e in source)
            {
                var (use, result) = conditionalSelector(e);
                if (use) yield return result;
            }
        }

        #region Set Operations

        /// <summary>
        /// Selects a distinct set of items based on the equality of the keys.
        /// </summary>
        public static IEnumerable<T> Distinct<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        {
            var comparer = new KeyEqualityComparer<T, TKey>(keySelector);
            return source.Distinct(comparer);
        }

        /// <summary>
        /// Set-equality (order-independent equality).
        /// </summary>
        /// <remarks>Order-independent counterpart to 
        /// <see cref="Enumerable.SequenceEqual{TSource}(IEnumerable{TSource}, IEnumerable{TSource}, IEqualityComparer{TSource})"/>.</remarks>
        public static bool SetEquals<T>(this IEnumerable<T> x, IEnumerable<T> y, IEqualityComparer<T> equalityComparer)
            => x.Count() == y.Count() && !x.Except(y, equalityComparer).Any() && !y.Except(x, equalityComparer).Any();

        /// <summary>
        /// Set-equality (order-independent equality).
        /// </summary>
        /// <remarks>Order-independent counterpart to 
        /// <see cref="Enumerable.SequenceEqual{TSource}(IEnumerable{TSource}, IEnumerable{TSource})"/>.</remarks>
        public static bool SetEquals<T>(this IEnumerable<T> x, IEnumerable<T> y)
            => x.Count() == y.Count() && !x.Except(y).Any() && !y.Except(x).Any();

        /// <summary>
        /// Generates a set-based (order-independent) hash code from the elements in the enumerable.  
        /// Equality counterpart to <see cref="SetEquals{T}(IEnumerable{T}, IEnumerable{T})"/>.
        /// Should only be used if immutability during use can be guaranteed.
        /// </summary>
        public static int HashCodeSet<T>(this IEnumerable<T> source) => source.Distinct().Aggregate(0, (n, t) => n ^ (t?.GetHashCode() ?? 0));

        /// <summary>
        /// Generates a set-based (order-independent) hash code from the elements in the enumerable.  
        /// Equality counterpart to <see cref="SetEquals{T}(IEnumerable{T}, IEnumerable{T}, IEqualityComparer{T})"/>.
        /// Should only be used if immutability during use can be guaranteed.
        /// </summary>
        public static int HashCodeSet<T>(this IEnumerable<T> source, IEqualityComparer<T> equalityComparer) 
            => source.Distinct(equalityComparer).Aggregate(0, (n, t) => n ^ equalityComparer.GetHashCode(t));

        /// <summary>
        /// Generates a sequence-based (order-dependent) hash code from the elements in the enumerable.  
        /// Equality counterpart to <see cref="Enumerable.SequenceEqual{TSource}(IEnumerable{TSource}, IEnumerable{TSource})"/>.
        /// Should only be used if immutability during use can be guaranteed.
        /// </summary>
        public static int HashCodeSequence<T>(this IEnumerable<T> source) 
            => source.Aggregate(0, (n, t) => (n, t).GetHashCode());

        /// <summary>
        /// Generates a sequence-based (order-dependent) hash code from the elements in the enumerable.  
        /// Equality counterpart to <see cref="Enumerable.SequenceEqual{TSource}(IEnumerable{TSource}, IEnumerable{TSource}, IEqualityComparer{TSource})"/>.
        /// Should only be used if immutability during use can be guaranteed.
        /// </summary>
        public static int HashCodeSequence<T>(this IEnumerable<T> source, IEqualityComparer<T> equalityComparer)
            => source.Aggregate(0, (n, t) => (n, equalityComparer.GetHashCode(t)).GetHashCode());

        #endregion Set

        #region Split

        /// <summary>
        /// Pages the enumerable into sub-enumerables with given page size. NOTA BENE: each page must be fully enumerated before proceeding.
        /// </summary>
        /// <remarks>
        /// This pages the enumerable without materializing or resetting. However, that requires that the enumerable-of-enumerables
        /// be consumed serially as it internally iterates the underlying enumerable.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the outer enumerable is iterated to the next page before the current page is enumerated.
        /// </exception>
        public static IEnumerable<IEnumerable<T>> Paginate<T>(this IEnumerable<T> source, int pageSize)
        {
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

            var enumerator = source.GetEnumerator();
            bool elementsRemain = enumerator.MoveNext();
            int currentPage = 0;

            IEnumerable<T> enumeratePage()
            {
                for (int i = 0; elementsRemain && i < pageSize; i++)
                {
                    yield return enumerator.Current;
                    elementsRemain = enumerator.MoveNext();
                }
                currentPage++;
            }

            for (int pageNumber = 0; elementsRemain; pageNumber++)
            {
                if (pageNumber != currentPage)
                    throw new InvalidOperationException($"Must fully enumerate page {currentPage} before proceeding to {pageNumber}");
                yield return enumeratePage();
            }
        }

        /// <summary>
        /// Split the enumerable into batches of the specified size.
        /// </summary>
        public static List<List<T>> SplitList<T>(this IEnumerable<T> listEnumerable, int sizeOfSplits)
        {
            if (sizeOfSplits <= 0) throw new ArgumentOutOfRangeException(nameof(sizeOfSplits));

            return listEnumerable.Paginate(sizeOfSplits).Select(p => p.ToList()).ToList();
        }

        /// <summary>
        /// An <see cref="Enumerable"/> that pulls the next `pageSize` items from the given Enumerator.
        /// </summary>
        /// <returns>
        /// An Enumerable{T} that is entangled with the given Enumerator. Not sure this is useful.
        /// </returns>
        public static IEnumerable<T> GetPage<T>(this IEnumerator<T> enumerator, int pageSize)
        {
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

            for (int i = 0; i < pageSize; i++)
            {
                if (!enumerator.MoveNext()) yield break;
                yield return enumerator.Current;
            }
        }

        #endregion Split 

        #region Any-continuation

        /// <summary>
        /// Like <see cref="Enumerable.Any{TSource}(IEnumerable{TSource})"/>, but allowing continuation ONCE after the initial check.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="singleUse">if non-empty, a single-use non-re-enumerating enumerable over `source`; otherwise <see cref="Enumerable.Empty{TResult}"/>.</param>
        /// <returns>Whether `source` has any elements; if so, a single-use, non-re-enumerating enumerable over `source`.</returns>
        /// <remarks>Helpful to avoid re-enumerating after an "any" check, but be careful not to reuse `singleUse`. Seriously.</remarks>
        public static bool AnyContinueOnce<T>(this IEnumerable<T> source, out IEnumerable<T> singleUse)
        {
            var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                singleUse = Enumerable.Empty<T>();
                return false;
            }

            IEnumerable<T> enumerateFromCurrent()
            {
                do yield return enumerator.Current;
                while (enumerator.MoveNext());
            }

            singleUse = enumerateFromCurrent();
            return true;
        }

        /// <summary>
        /// Returns `null` as an empty signal, without re-enumerating the first element as with an `Any()` check.
        /// </summary>
        /// <remarks>
        /// See <see cref="Safe{T}(IEnumerable{T})"/>; within a processing block, non-null
        /// enumerable semantics should be retained. Transforming empty results to null (for instance,
        /// to skip the JSON output of an empty list-element) should be kept as close to the
        /// "right edge" as possible.
        /// </remarks>
        public static IEnumerable<T>? NullIfEmpty<T>(this IEnumerable<T> source)
            => !source.AnyContinueOnce(out var ifAny) ? null : ifAny;

        /// <summary>
        /// Allows aggregation on an empty enumerable.
        /// </summary>
        public static T? SafeAggregate<T>(this IEnumerable<T> source, Func<T, T, T> func)
            => source == null || !source.AnyContinueOnce(out var ifAny) ? default : ifAny.Aggregate(func);

        /// <summary>
        /// Returns a null list if empty.
        /// </summary>
        /// <remarks>
        /// See <see cref="Safe{T}(IEnumerable{T})"/>; within a processing block, non-null
        /// enumerable semantics should be retained. Transforming empty results to null (for instance,
        /// to skip the JSON output of an empty list-element) should be kept as close to the
        /// "right edge" as possible.
        /// </remarks>
        public static List<T>? NullIfEmptyToList<T>(this IEnumerable<T> source)
            => source == null || !source.AnyContinueOnce(out var ifAny) ? null : ifAny.ToList();

        #endregion
    }

    internal sealed class KeyEqualityComparer<T, TKey> : IEqualityComparer<T>
    {
        private readonly IEqualityComparer<TKey> _equalityComparer;
        private readonly Func<T, TKey> _keySelector;

        public KeyEqualityComparer(Func<T, TKey> keySelector)
            : this(keySelector, EqualityComparer<TKey>.Default)
        {
        }

        public KeyEqualityComparer(Func<T, TKey> keySelector, IEqualityComparer<TKey> equalityComparer)
        {
            _keySelector = keySelector;
            _equalityComparer = equalityComparer;
        }

        public bool Equals(T x, T y)
        {
            return _equalityComparer.Equals(_keySelector(x), _keySelector(y));
        }

        public int GetHashCode(T obj)
        {
            return _equalityComparer.GetHashCode(_keySelector(obj));
        }
    }

}
