using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelMerge
{
    public class ExcelSheetDiffConfig
    {
        public int SrcSheetIndex { get; set; }
        public int DstSheetIndex { get; set; }
        public int SrcHeaderIndex { get; set; }
        public int DstHeaderIndex { get; set; }
    }
}
