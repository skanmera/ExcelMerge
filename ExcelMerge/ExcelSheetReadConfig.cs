using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelMerge
{
    public class ExcelSheetReadConfig
    {
        public bool TrimFirstBlankRows { get; set; }
        public bool TrimFirstBlankColumns { get; set; }
        public bool TrimLastBlankRows { get; set; }
        public bool TrimLastBlankColumns { get; set; }
    }
}
