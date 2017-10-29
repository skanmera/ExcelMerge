using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection;

namespace ExcelMerge.GUI.ValueConverters
{
    public class FileDialogParameter
    {
        public object Obj { get; private set; }
        public PropertyInfo PropertyInfo { get; private set; }
        public string Title { get; set; } = "Open File";

        public FileDialogParameter(object obj, PropertyInfo propertyInfo)
        {
            Obj = obj;
            PropertyInfo = propertyInfo;
        }
    }
}
