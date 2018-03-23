using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKCore.Collection
{
    public static partial class EnumerableExtension
    {
        public static int MaxOrDefault<T>(
            this IEnumerable<T> source, Func<T, int> selector, int defaultValue = 0)
        {
            return source.Any() ? source.Max(selector) : defaultValue;
        }
    }
}
