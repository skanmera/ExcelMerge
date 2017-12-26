using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastWpfGrid
{
    public class HoverRowChangedEventArgs : EventArgs
    {
        public FastGridCellAddress Cell { get; }

        public HoverRowChangedEventArgs(FastGridCellAddress cell)
        {
            Cell = cell;
        }
    }
}
