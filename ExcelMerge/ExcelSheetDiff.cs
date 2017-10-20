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

        public ExcelSheetDiffSummary CreateSummary()
        {
            var addedRowCount = 0;
            var removedRowCount = 0;
            var modifiedRowCount = 0;
            var modifiedCellCount = 0;
            foreach (var row in Rows)
            {
                if (row.Value.Added())
                    addedRowCount++;
                else if (row.Value.Removed())
                    removedRowCount++;

                if (row.Value.Modified())
                    modifiedRowCount++;

                modifiedCellCount += row.Value.ModifiedCellCount;
            }

            return new ExcelSheetDiffSummary
            {
                AddedRowCount = addedRowCount,
                RemovedRowCount = removedRowCount,
                ModifiedRowCount = modifiedRowCount,
                ModifiedCellCount = modifiedCellCount,
            };
        }
    }
}
