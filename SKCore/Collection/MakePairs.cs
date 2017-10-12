using System;
using System.Collections.Generic;
using System.Linq;

namespace SKCore.Collection
{
    public static partial class EnumerableExtension
    {
        public static IEnumerable<Tuple<T, T>> MakePairs<T>(this IEnumerable<T> source)
        {
            return source.SplitByRegularity((items, current) => { items.Add(current); return false; })
                .Where(i => i.Count() > 1)
                .Select(i => Tuple.Create(i.ElementAt(0), i.ElementAt(1)));
        }
    }
}
