using System.Collections.Generic;

namespace NetDiff
{
    public static class DiffResultExtension
    {
        public static IEnumerable<T> CreateSrc<T>(
            this IEnumerable<DiffResult<T>> self)
        {
            return DiffUtil.CreateSrc(self);
        }

        public static IEnumerable<T> CreateDst<T>(
            this IEnumerable<DiffResult<T>> self)
        {
            return DiffUtil.CreateDst(self);
        }

        public static IEnumerable<DiffResult<T>> Optimize<T>(
            this IEnumerable<DiffResult<T>> self, IEqualityComparer<T> compare = null)
        {
            return DiffUtil.Optimaize(self, compare);
        }

        public static IEnumerable<DiffResult<T>> Order<T>(
            this IEnumerable<DiffResult<T>> self, DiffOrderType orderType)
        {
            return DiffUtil.Order(self, orderType);
        }
    }
}
