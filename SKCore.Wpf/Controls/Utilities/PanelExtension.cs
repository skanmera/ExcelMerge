using System.Collections.Generic;
using System.Linq; 
using System.Windows.Controls;
using System.Windows;

namespace SKCore.Wpf.Controls.Utilities
{
    public static class PanelExtension
    {
        public static int ClearChildren<T>(this Panel self)
        {
            return self.ClearChildren<T>(Enumerable.Empty<UIElement>());
        }

        public static int ClearChildren<T>(this Panel self, IEnumerable<UIElement> ignoreElements)
        {
            var removeCount = 0;
            for (int i = self.Children.Count - 1; i >= 0; i--)
            {
                var element = self.Children[i];
                if (element.GetType() == typeof(T) && !ignoreElements.Contains(element))
                {
                    self.Children.Remove(element);
                    removeCount++;
                }
            }

            return removeCount;
        }
    }
}
