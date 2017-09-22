using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastWpfGrid
{
    public class RowClickEventArgs : EventArgs
    {
        public int Row;
        public FastGridControl Grid;
        public bool Handled;
    }

    public class ColumnClickEventArgs : EventArgs
    {
        public int Column;
        public FastGridControl Grid;
        public bool Handled;
    }
}
