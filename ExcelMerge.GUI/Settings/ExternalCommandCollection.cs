using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelMerge.GUI.Settings
{
    [Serializable]
    public class ExternalCommandCollection : ObservableCollection<ExternalCommand>
    {
        public ExternalCommandCollection() : base() { }
        public ExternalCommandCollection(IEnumerable<ExternalCommand> commands) : base(commands) { }
    }
}
