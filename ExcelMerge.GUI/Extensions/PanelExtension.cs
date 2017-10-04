using System.Windows.Controls;

namespace ExcelMerge.GUI.Extensions
{
    static class PanelExtension
    {
        public static int ClearChildren<T>(this Panel self)
        {
            var removeCount = 0;
            for (int i = self.Children.Count - 1; i > 0; i--)
            {
                if (self.Children[i].GetType() == typeof(T))
                {
                    var ucCurrentChild = self.Children[i];
                    self.Children.Remove(ucCurrentChild);
                    removeCount++;
                }
            }

            return removeCount;
        }
    }
}
