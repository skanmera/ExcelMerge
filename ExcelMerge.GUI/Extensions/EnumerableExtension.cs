using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelMerge.GUI.Extensions
{
    public static class EnumerableExtension
    {
        public static IEnumerable<IEnumerable<T>> SplitByComparison<T>(this IEnumerable<T> source)
        {
            return source.SplitByComparison((current, next) => current.Equals(next));
        }

        public static IEnumerable<IEnumerable<T>> SplitByComparison<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
        {
            return source.SplitByComparison((current, next) => comparer.Equals(current, next));
        }

        public static IEnumerable<IEnumerable<T>> SplitByComparison<T>(this IEnumerable<T> source, Func<T, T, bool> predicate)
        {
            var queue = new Queue<T>(source);
            var items = new List<T>();
            while (queue.Any())
            {
                var current = queue.Dequeue();
                items.Add(current);

                if (queue.Any() && predicate(current, queue.Peek()))
                    continue;

                yield return items.AsEnumerable();

                items = new List<T>();
            }
        }
    }
}
