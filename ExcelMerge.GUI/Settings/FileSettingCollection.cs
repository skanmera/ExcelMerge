using System;
using System.Collections.Generic;

namespace ExcelMerge.GUI.Settings
{
    [Serializable]
    public class FileSettingCollection : SettingCollection<FileSetting>
    {
        public FileSettingCollection() : base() { }
        public FileSettingCollection(IEnumerable<FileSetting> fileSettings)
            : base(fileSettings)
        { }
    }
}
