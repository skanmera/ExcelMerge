using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKCore.Collection
{
    public static partial class EnumerableExtension
    {
        public static T ElementAtOrElse<T>(this IEnumerable<T> source, int index, T value)
        {
            return source.Count() > index ? source.ElementAt(index) : value;
        }
    }
}
