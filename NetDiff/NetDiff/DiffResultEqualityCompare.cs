using System.Collections.Generic;

namespace NetDiff
{
    public class DiffResultEqualityCompare<T> : IEqualityComparer<DiffResult<T>>
    {
        public bool Equals(DiffResult<T> x, DiffResult<T> y)
        {
            if (x == null)
            {
                if (y == null)
                    return true;
                else
                    return false;
            }

            var isEqualObj1 = x.Obj1 != null ? x.Obj1.Equals(y.Obj1) : y.Obj1 == null;
            var isEqualObj2 = x.Obj2 != null ? x.Obj2.Equals(y.Obj2) : y.Obj2 == null;

            return isEqualObj1 && isEqualObj2 && x.Status == y.Status;
        }

        public int GetHashCode(DiffResult<T> obj)
        {
            return obj.GetHashCode();
        }
    }
}
