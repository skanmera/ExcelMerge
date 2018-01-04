using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastWpfGrid
{
    public class FontSizeChangedEventArgs : EventArgs
    {
        public int NewSize { get; }

        public FontSizeChangedEventArgs(int newSize)
        {
            NewSize = newSize;
        }
    }
}
