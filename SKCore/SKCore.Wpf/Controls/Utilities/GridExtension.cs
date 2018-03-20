using System;
using System.Windows;
using System.Windows.Controls;

namespace SKCore.Wpf.Controls.Utilities
{
    public static class GridExtension
    {
        public static int? GetRow(this Grid self, Point relativePoint)
        {
            if (relativePoint.Y < 0)
                return null;

            double height = 0.0;
            int row = 0;
            foreach (RowDefinition rd in self.RowDefinitions)
            {
                height += rd.Height.IsAbsolute ? rd.Height.Value : rd.ActualHeight;
                if (relativePoint.Y < height)
                    return row;

                row++;
            }

            return null;
        }

        public static int? GetColumn(this Grid self, Point relativePoint)
        {
            if (relativePoint.X < 0)
                return null;

            double width = 0.0;
            int column = 0;
            foreach (ColumnDefinition rd in self.ColumnDefinitions)
            {
                width += rd.Width.IsAbsolute ? rd.Width.Value : rd.ActualWidth;
                if (relativePoint.X < width)
                    return column;

                column++;
            }

            return null;
        }
    }
}
