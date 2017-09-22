namespace ExcelMerge
{
    public class ExcelCellDiff
    {
        public int ColumnIndex { get; }
        public int RowIndex { get; }
        public ExcelCell SrcCell { get; }
        public ExcelCell DstCell { get; }
        public ExcelCellStatus Status { get; }

        public ExcelCellDiff(int columnIndex, int rowIndex, ExcelCell src, ExcelCell dst, ExcelCellStatus status)
        {
            ColumnIndex = columnIndex;
            RowIndex = rowIndex;
            SrcCell = src;
            DstCell = dst;
            Status = status;
        }

        public override string ToString()
        {
            return $"Src: {SrcCell.Value} Dst: {DstCell.Value}: Status: {Status}";
        }
    }
}
