using System.Windows.Input;
using System.Windows.Controls;

namespace ExcelMerge.GUI.Extensions
{
    internal static class GridExtension
    {
        public static int GetRow(this Grid self, MouseEventArgs e)
        {
            double y = e.GetPosition(self).Y;
            double start = 0.0;
            int row = 0;
            foreach (RowDefinition rd in self.RowDefinitions)
            {
                start += rd.ActualHeight;
                if (y < start)
                    break;

                row++;
            }

            return row;
        }

        public static int GetColumn(this Grid self, MouseEventArgs e)
        {
            double x = e.GetPosition(self).X;
            double start = 0.0;
            int column = 0;
            foreach (ColumnDefinition rd in self.ColumnDefinitions)
            {
                start += rd.ActualWidth;
                if (x < start)
                    break;

                column++;
            }

            return column;
        }
    }
}
