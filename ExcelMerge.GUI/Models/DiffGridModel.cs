using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using FastWpfGrid;
using ExcelMerge.GUI.Styles;

namespace ExcelMerge.GUI.Models
{
    public class DiffGridModel : FastGridModelBase
    {
        private int columnCount;
        private int rowCount;
        private Color? backgroundColor = null;
        private Color? decorationColor = null;
        private CellDecoration decoration = CellDecoration.None;

        public override int ColumnCount
        {
            get { return columnCount; }
        }

        public override int RowCount
        {
            get { return rowCount; }
        }

        public override Color? BackgroundColor
        {
            get { return backgroundColor; }
        }

        public override Color? DecorationColor
        {
            get { return decorationColor; }
        }

        public override CellDecoration Decoration
        {
            get { return decoration; }
        }

        public int HeaderIndex { get; private set; }
        public DiffType DiffType { get; private set; }
        public ExcelSheetDiff SheetDiff { get; private set; }
        public DiffGridModelConfig Config { get; private set; }
        public Dictionary<string, Color?> ColorTable { get; private set; }

        public DiffGridModel(DiffType type, ExcelSheetDiff sheetDiff, DiffGridModelConfig config) : base()
        {
            SheetDiff = sheetDiff;
            Config = config;
            HeaderIndex = Config.HeaderIndex;
            ColorTable = new Dictionary<string, Color?>(config.ColorTable);
            DiffType = type;

            columnCount = SheetDiff.Rows.Max(r => r.Value.Cells.Count);
            rowCount = SheetDiff.Rows.Count();
        }

        public override string GetColumnHeaderText(int column)
        {
            return GetCellText(HeaderIndex, column);
        }

        public override string GetCellText(int row, int column)
        {
            ExcelCellDiff cellDiff;
            if (TryGetCellDiff(row, column, out cellDiff))
                return GetCellText(cellDiff);

            return string.Empty;
        }

        private bool TryGetCellDiff(int row, int column, out ExcelCellDiff cellDiff)
        {
            cellDiff = null;

            ExcelRowDiff rowDiff;
            if (SheetDiff.Rows.TryGetValue(row, out rowDiff))
                return rowDiff.Cells.TryGetValue(column, out cellDiff);

            return false;
        }

        private string GetCellText(ExcelCellDiff cellDiff)
        {
            switch (cellDiff.Status)
            {
                case ExcelCellStatus.None:
                    return cellDiff.SrcCell.Value;
                case ExcelCellStatus.Modified:
                    return DiffType == DiffType.Source ? cellDiff.SrcCell.Value : cellDiff.DstCell.Value;
                case ExcelCellStatus.Added:
                    return DiffType == DiffType.Source ? cellDiff.SrcCell.Value : cellDiff.DstCell.Value;
                case ExcelCellStatus.Removed:
                    return DiffType == DiffType.Source ? cellDiff.SrcCell.Value : cellDiff.DstCell.Value;
            }

            return string.Empty;
        }

        private Color? GetColor(string key)
        {
            Color? color;
            ColorTable.TryGetValue(key, out color);

            return color;
        }

        public override IFastGridCell GetRowHeader(IFastGridView view, int row)
        {
            var header = base.GetRowHeader(view, row) as DiffGridModel;
            if (header == null)
                return header;

            header.backgroundColor = GetColor("RowHeader");

            return header;
        }

        public override IFastGridCell GetCell(IFastGridView view, int row, int column)
        {
            var cell = base.GetCell(view, row, column) as DiffGridModel;
            if (cell == null)
                return cell;

            ExcelCellDiff cellDiff;
            var status = ExcelCellStatus.None;
            if (TryGetCellDiff(row, column, out cellDiff))
            {
                status = cellDiff.Status;
                if (status == ExcelCellStatus.Added && DiffType == DiffType.Source)
                    status = ExcelCellStatus.Removed;
                else if (status == ExcelCellStatus.Removed && DiffType == DiffType.Source)
                    status = ExcelCellStatus.Added;
            }

            cell.backgroundColor = null;

            if (cell.IsFrozenColulmn(column))
                cell.backgroundColor = EMColor.LightPink;

            cell.backgroundColor = GetColor(status.ToString()) ?? cell.backgroundColor;

            return cell;
        }

        public void FreezeColumn(int? column)
        {
            UnfreezeColumn();

            if (column.HasValue)
                SetColumnArrange(new HashSet<int>(), new HashSet<int>(Enumerable.Range(0, column.Value + 1)));
        }

        public void UnfreezeColumn()
        {
            SetColumnArrange(new HashSet<int>(), new HashSet<int>());
        }

        public void SetHeader(int? row)
        {
            if (row.HasValue)
                HeaderIndex = row.Value;
        }
    }
}
