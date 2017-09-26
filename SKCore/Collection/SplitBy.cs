using System;
using System.Collections.Generic;
using System.Linq;

namespace SKCore.Collection
{
    public static partial class EnumerableExtension
    {
        public static IEnumerable<IEnumerable<T>> SplitBySize<T>(
            this IEnumerable<T> source, int size)
        {
            return source.SplitByRegularity((p, c, gi, li) => li < size);
        }

        public static IEnumerable<IEnumerable<T>> SplitByEquality<T>(
            this IEnumerable<T> source)
        {
            return source.SplitByRegularity((p, c, gi, li) => c.Equals(p));
        }

        public static IEnumerable<IEnumerable<T>> SplitByEquality<T>(
            this IEnumerable<T> source, int maxSize)
        {
            return source.SplitByRegularity((p, c, gi, li) => c.Equals(p) && li < maxSize);
        }

        public static IEnumerable<IEnumerable<T>> SplitByEquality<T>(
            this IEnumerable<T> source, IEqualityComparer<T> comparer)
        {
            return source.SplitByRegularity((p, c, gi, li) => comparer.Equals(c, p));
        }

        public static IEnumerable<IEnumerable<T>> SplitByEquality<T>(
            this IEnumerable<T> source, IEqualityComparer<T> comparer, int maxSize)
        {
            return source.SplitByRegularity((p, c, gi, li) => comparer.Equals(c, p) && li <= maxSize);
        }

        public static IEnumerable<IEnumerable<T>> SplitByRegularity<T>(
            this IEnumerable<T> source, Func<T, T, int, int, bool> predicate)
        {
            using (var enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    yield break;

                var items = new List<T> { enumerator.Current };
                var localIndex = 0;
                var globalIndex = 0;
                var previous = default(T);
                while (true)
                {
                    previous = enumerator.Current;
                    if (!enumerator.MoveNext())
                        break;

                    if (predicate(previous, enumerator.Current, ++globalIndex, ++localIndex))
                    {
                        items.Add(enumerator.Current);
                        continue;
                    }

                    yield return items;

                    localIndex = 0;
                    items = new List<T> { enumerator.Current };
                }

                if (items.Any())
                    yield return items;
            }
        }
    }
}

