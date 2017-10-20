using System.Linq;
using System.Collections.Generic;

namespace ExcelMerge
{
    public class ExcelRowDiff
    {
        public int Index { get; private set; }
        public SortedDictionary<int, ExcelCellDiff> Cells { get; private set; }

        public ExcelRowDiff(int index)
        {
            Index = index;
            Cells = new SortedDictionary<int, ExcelCellDiff>();
        }

        public ExcelCellDiff CreateCell(ExcelCell src, ExcelCell dst, int columnIndex, ExcelCellStatus status)
        {
            var cell = new ExcelCellDiff(columnIndex, Index, src, dst, status);
            Cells.Add(cell.ColumnIndex, cell);

            return cell;
        }

        public bool Modified()
        {
            return Cells.Any(c => c.Value.Status != ExcelCellStatus.None);
        }

        public bool Added()
        {
            return Cells.All(c => c.Value.Status == ExcelCellStatus.Added);
        }

        public bool Removed()
        {
            return Cells.All(c => c.Value.Status == ExcelCellStatus.Removed);
        }

        public int ModifiedCellCount
        {
            get { return Cells.Count(c => c.Value.Status != ExcelCellStatus.None); }
        }
    }
}
