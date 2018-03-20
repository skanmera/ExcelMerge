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
            return source.SplitByRegularity((items, current) => items.Count < size);
        }

        public static IEnumerable<IEnumerable<T>> SplitByEquality<T>(
            this IEnumerable<T> source)
        {
            return source.SplitByRegularity((items, current) => items.Last().Equals(current));
        }

        public static IEnumerable<IEnumerable<T>> SplitByEquality<T>(
            this IEnumerable<T> source, int maxSize)
        {
            return source.SplitByRegularity((items, current) => items.Last().Equals(current) && items.Count < maxSize);
        }

        public static IEnumerable<IEnumerable<T>> SplitByEquality<T>(
            this IEnumerable<T> source, IEqualityComparer<T> comparer)
        {
            return source.SplitByRegularity((items, current) => comparer.Equals(items.Last(), current));
        }

        public static IEnumerable<IEnumerable<T>> SplitByEquality<T>(
            this IEnumerable<T> source, IEqualityComparer<T> comparer, int maxSize)
        {
            return source.SplitByRegularity((items, current) => comparer.Equals(items.Last(), current) && items.Count <= maxSize);
        }

        public static IEnumerable<IEnumerable<T>> SplitByRegularity<T>(
            this IEnumerable<T> source, Func<List<T>, T, bool> predicate)
        {
            using (var enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    yield break;

                var items = new List<T> { enumerator.Current };
                while (enumerator.MoveNext())
                {
                    if (predicate(items, enumerator.Current))
                    {
                        items.Add(enumerator.Current);
                        continue;
                    }

                    yield return items;

                    items = new List<T> { enumerator.Current };
                }

                if (items.Any())
                    yield return items;
            }
        }
    }
}

