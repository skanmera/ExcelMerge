using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelMerge.GUI.Settings
{
    [Serializable]
    public class FileSettingCollection : ObservableCollection<FileSetting>
    {
        public FileSettingCollection() : base() { }
        public FileSettingCollection(IEnumerable<FileSetting> settings) : base(settings) { }
    }
}
