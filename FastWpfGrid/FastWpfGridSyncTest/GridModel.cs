using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FastWpfGrid;

namespace FastWpfGridSyncTest
{
    public class GridModel : FastGridModelBase
    {
        private int columnCount;
        private int rowCount;
        private Dictionary<int, Dictionary<int, object>> table;

        public override int ColumnCount
        {
            get { return table.Any() ? table.First().Value.Count : 0; }
        }

        public override int RowCount
        {
            get { return table.Count; }
        }

        public GridModel() : base()
        {
            table = new Dictionary<int, Dictionary<int, object>>();

            for (int i = 0; i < 100; i++)
            {
                var row = new Dictionary<int, object>();
                for (int j = 0; j < 100; j++)
                {
                    row.Add(j, $"{i},{j}");
                }

                table.Add(i, row);
            }
        }

        public override string GetCellText(int row, int column)
        {
            object obj = null;
            Dictionary<int, object> record;
            if (table.TryGetValue(row, out record))
                record.TryGetValue(column, out obj);

            return obj?.ToString() ?? string.Empty;
        }

        public override IFastGridCell GetCell(IFastGridView view, int row, int column)
        {
            return base.GetCell(view, row, column);
        }
    }
}
