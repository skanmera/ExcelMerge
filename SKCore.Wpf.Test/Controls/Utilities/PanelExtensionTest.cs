using System;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SKCore.Wpf.Controls.Utilities;

namespace SKCore.Wpf.Test.Controls.Utilities
{
    [TestClass]
    public class PanelExtension 
    {
        [TestMethod]
        public void ClearChildrenTest()
        {
            var panel = new StackPanel();
            panel.Children.Add(new Label());
            panel.Children.Add(new Label());
            panel.Children.Add(new TextBox());

            panel.ClearChildren<Label>();

            var label= 0;
            var textBox = 0;
            foreach (var c in panel.Children)
            {
                if (c.GetType() == typeof(Label))
                    label++;
                else if (c.GetType() == typeof(TextBox))
                    textBox++;
            }

            Assert.AreEqual(0, label);
            Assert.AreEqual(1, textBox);
        }
    }
}
