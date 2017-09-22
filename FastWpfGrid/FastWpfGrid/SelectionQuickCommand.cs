using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastWpfGrid
{
    public class SelectionQuickCommand
    {
        public string Text { get; set; }
        public IFastGridModel Model { get; set; }

        public SelectionQuickCommand(IFastGridModel model, string text)
        {
            Text = text;
            Model = model;
        }
    }
}
