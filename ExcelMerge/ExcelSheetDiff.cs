using System.Collections.Generic;
using System.Linq;

namespace ExcelMerge
{
    public class ExcelSheetDiff
    {
        public SortedDictionary<int, ExcelRowDiff> Rows { get; private set; }

        public ExcelSheetDiff()
        {
            Rows = new SortedDictionary<int, ExcelRowDiff>();
        }

        public ExcelRowDiff CreateRow()
        {
            var row = new ExcelRowDiff(Rows.Any() ? Rows.Keys.Last() + 1 : 0);
            Rows.Add(row.Index, row);

            return row;
        }
    }
}
