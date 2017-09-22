using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FastWpfGridTest
{
    public class TextRenderPanel : Panel
    {
        protected override void OnRender(System.Windows.Media.DrawingContext dc)
        {
            base.OnRender(dc);
            var bgColor = Color.FromRgb(0xF9, 0xF9, 0xF9);
            dc.DrawRectangle(new SolidColorBrush(bgColor), null, new Rect(0, 0, ActualWidth, ActualHeight));
            var text = new FormattedText("ABC l 123 Gfijdr", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 12, Brushes.Black);
            dc.DrawText(text, new Point(0, 0));
            dc.DrawLine(new Pen(Brushes.Black, 1), new Point(0.5, 10.5), new Point(50.5, 10.5));
            dc.DrawLine(new Pen(Brushes.Black, 1), new Point(30.5, 0.5), new Point(30.5, 40.5));
        }
    }
}
