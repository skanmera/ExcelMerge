using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using FastWpfGrid;

namespace FastWpfGridTest
{
    public class GridModel3 : FastGridModelBase
    {
        private string _longText =
            "Lewis Hamilton took a stunning pole position for Mercedes at the season-opening Australian Grand Prix as McLaren-Honda qualified last. The world champion beat team-mate Nico Rosberg by more than half a second as Mercedes utterly dominated.  Williams's Felipe Massa beat Ferrari's Sebastian Vettel to third. Meanwhile, McLaren produced their worst performance for six years as their big-budget new engine partnership with Honda got off to a terrible start. ";

        public override int ColumnCount
        {
            get { return 5; }
        }

        public override int RowCount
        {
            get { return 1000; }
        }

        public override IFastGridCell GetCell(IFastGridView view, int row, int column)
        {
                    var cell = new FastGridCellImpl();
            switch (column)
            {
                case 2:
                    var lines = new List<string>();
                    for (int i = 0; i <= row%5; i++)
                    {
                        lines.Add(String.Format("Line {0}", i));
                    }

                    if (view.FlexibleRows)
                    {
                        cell.AddTextBlock(String.Join("\n", lines));
                    }
                    else
                    {
                        cell.AddTextBlock(String.Format("({0} lines)", lines.Count)).FontColor = Colors.Gray;
                        cell.ToolTipText = String.Join("\n", lines);
                    }
                    return cell;

                case 3:
                    cell.AddTextBlock(_longText);
                    cell.ToolTipText = _longText;
                    cell.ToolTipVisibility = TooltipVisibilityMode.OnlyWhenTrimmed;
                    return cell;

                default:
                    return base.GetCell(view, row, column);
            }
        }
    }
}
