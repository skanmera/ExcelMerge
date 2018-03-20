using System;
using System.Collections.Generic;
using System.Linq;

namespace SKCore.Collection
{
    public static partial class EnumerableExtension
    {
        public static bool NestedSequenceEqual<T>(
            this IEnumerable<IEnumerable<T>> first, IEnumerable<IEnumerable<T>> second)
        {
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            var firstCount = first.Count();
            var secondCount = second.Count();
            if (firstCount != secondCount)
                return false;

            for (int i = 0; i < firstCount; i++)
            {
                if (!first.ElementAt(i).SequenceEqual(second.ElementAt(i)))
                    return false;
            }

            return true;
        }
    }
}
