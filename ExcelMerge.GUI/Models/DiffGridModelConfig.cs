using System.Collections.Generic;
using System.Windows.Media;
using ExcelMerge.GUI.Styles;

namespace ExcelMerge.GUI.Models
{
    public class DiffGridModelConfig
    {
        public static Dictionary<string, Color?> DefaultColorTable
        {
            get
            {
                return new Dictionary<string, Color?>
                {
                    { "None", null},
                    { "Modified", EMColor.Orange},
                    { "Added", EMColor.Orange},
                    { "Removed", EMColor.LightGray},
                    { "ColumnHeader", EMColor.LightPink},
                    { "RowHeader", EMColor.LightPink},
                };
            }
        }

        public int HeaderIndex { get; set; }
        public int FrozenColumnIndex { get; set; }
        public Dictionary<string, Color?> ColorTable { get; private set; } = DefaultColorTable;
    }
}
