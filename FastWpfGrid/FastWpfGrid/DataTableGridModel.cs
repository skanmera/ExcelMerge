using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastWpfGrid
{
    public class DataTableGridModel : FastGridModelBase
    {
        private DataTable _dataSource;
        public List<ExplicitColumnDefinition> ExplicitColumns;

        public override int ColumnCount
        {
            get
            {
                if (ExplicitColumns != null) return ExplicitColumns.Count;
                if (_dataSource != null) return _dataSource.Columns.Count;
                return 0;
            }
        }

        public override int RowCount
        {
            get { return _dataSource != null ? _dataSource.Rows.Count : 0; }
        }

        public override string GetCellText(int row, int column)
        {
            if (_dataSource != null && row < _dataSource.Rows.Count)
            {
                if (ExplicitColumns != null)
                {
                    if (column < ExplicitColumns.Count)
                    {
                        object value = _dataSource.Rows[row][ExplicitColumns[column].DataField];
                        if (value != null) return value.ToString();
                    }
                }
                else
                {
                    object value = _dataSource.Rows[row][column];
                    if (value != null) return value.ToString();
                }
            }
            return "";
        }

        public override string GetColumnHeaderText(int column)
        {
            if (ExplicitColumns != null)
            {
                if (column < ExplicitColumns.Count) return ExplicitColumns[column].HeaderText;
                return "";
            }
            if (_dataSource != null && column < _dataSource.Columns.Count)
            {
                return _dataSource.Columns[column].ColumnName;
            }
            return "";
        }

        public DataTable DataSource
        {
            get { return _dataSource; }
            set
            {
                _dataSource = value;
                NotifyRefresh();
            }
        }
    }
}
