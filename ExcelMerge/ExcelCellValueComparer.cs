using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelMerge
{
    public class ExcelCellValueComparer : IEqualityComparer<ExcelCell>
    {
        public bool Equals(ExcelCell x, ExcelCell y)
        {
            if (x == null || y == null)
                return false;

            return x.Value.Equals(y.Value);
        }

        public int GetHashCode(ExcelCell obj)
        {
            return obj.Value.GetHashCode();
        }
    }
}
