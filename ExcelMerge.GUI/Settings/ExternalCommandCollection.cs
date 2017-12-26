using System;
using System.Collections.Generic;

namespace ExcelMerge.GUI.Settings
{
    [Serializable]
    public class ExternalCommandCollection : SettingCollection<ExternalCommand>
    {
        public ExternalCommandCollection() : base() { }
        public ExternalCommandCollection(IEnumerable<ExternalCommand> externalCommands)
            : base(externalCommands)
        { }
    }
}
