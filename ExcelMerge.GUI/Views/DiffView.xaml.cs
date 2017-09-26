using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using FastWpfGrid;
using NetDiff;
using ExcelMerge.GUI.Extensions;
using ExcelMerge.GUI.Models;
using ExcelMerge.GUI.Styles;

namespace ExcelMerge.GUI.Views
{
    public partial class DiffView : UserControl
    {
        private const double minimumLocationGridSize = 5d;
        private double valueTextBoxHeight;
        private bool updateViewRectangleEnabled;
        private ExcelSheetDiffConfig diffConfig;

        public DiffView()
        {
            InitializeComponent();

            FastGridControl.RegisterSyncScrollGrid("DataGrid", SrcDataGrid);
            FastGridControl.RegisterSyncScrollGrid("DataGrid", DstDataGrid);

            SrcDataGrid.AlternatingColors = new Color[] { Colors.White, Color.FromRgb(250, 250, 250) };
            DstDataGrid.AlternatingColors = new Color[] { Colors.White, Color.FromRgb(250, 250, 250) };

            SrcDataGrid.SetMaxRowSize(100);
            DstDataGrid.SetMaxRowSize(100);
            SrcDataGrid.SetMaxColumnSize(100);
            DstDataGrid.SetMaxColumnSize(100);
            SrcDataGrid.SetMinColumnSize(100);
            DstDataGrid.SetMinColumnSize(100);

            valueTextBoxHeight = SrcValueTextBox.Height;
            updateViewRectangleEnabled = true;
        }

        private void DstDataGrid_Scrolled(object sender, EventArgs e)
        {
            SyncScroll(DstDataGrid, SrcDataGrid);

            UpdateViewRectangle(SrcViewRectangle, SrcDataGrid);
            UpdateViewRectangle(DstViewRectangle, DstDataGrid);

            SrcDataGrid.NotifyRefresh();
            DstDataGrid.NotifyRefresh();
        }

        private void SrcDataGrid_Scrolled(object sender, EventArgs e)
        {
            SyncScroll(SrcDataGrid, DstDataGrid);

            UpdateViewRectangle(SrcViewRectangle, SrcDataGrid);
            UpdateViewRectangle(DstViewRectangle, DstDataGrid);

            SrcDataGrid.NotifyRefresh();
            DstDataGrid.NotifyRefresh();
        }

        private void SyncScroll(FastGridControl scrolled, FastGridControl other)
        {
            if (scrolled.VerticalScrollBarOffset == other.VerticalScrollBarOffset &&
                scrolled.HorizontalScrollBarOffset == other.HorizontalScrollBarOffset)
                return;

            var row = (int)Math.Round(scrolled.VerticalScrollBarOffset);
            var col = (int)Math.Round(scrolled.HorizontalScrollBarOffset);

            other.Scroll(row, col, scrolled.VerticalScrollBarOffset, scrolled.HorizontalScrollBarOffset);
        }

        private bool UpdateLocationGridDefinitions(Grid locationGrid, FastGridControl dataGrid, Size size)
        {
            if (dataGrid == null || dataGrid.Model == null)
                return false;

            var width = dataGrid.Model.ColumnCount > 0 ? size.Width / dataGrid.Model.ColumnCount : 0;
            var height = dataGrid.Model.RowCount > 0 ? size.Height / dataGrid.Model.RowCount : 0;

            locationGrid.ColumnDefinitions.Clear();
            for (int i = 0; i < dataGrid.Model.ColumnCount; i++)
            {
                var colDef = new ColumnDefinition()
                {
                    Width = new GridLength(width, GridUnitType.Pixel),
                };

                locationGrid.ColumnDefinitions.Add(colDef);
            }

            locationGrid.RowDefinitions.Clear();
            for (int i = 0; i < dataGrid.Model.RowCount; i++)
            {
                var rowDef = new RowDefinition()
                {
                    Height = new GridLength(height, GridUnitType.Pixel),
                };

                locationGrid.RowDefinitions.Add(rowDef);
            }

            return true;
        }

        private void UpdateViewRectangle(Rectangle viewRectangle, FastGridControl dataGrid)
        {
            if (!updateViewRectangleEnabled)
                return;

            if (dataGrid.VisibleColumnCount <= 0 || dataGrid.VisibleRowCount <= 0)
                return;

            Grid.SetColumn(viewRectangle, dataGrid.FirstVisibleColumnScrollIndex);
            Grid.SetColumnSpan(viewRectangle, dataGrid.VisibleColumnCount);
            Grid.SetRow(viewRectangle, dataGrid.FirstVisibleRowScrollIndex);
            Grid.SetRowSpan(viewRectangle, dataGrid.VisibleRowCount);
        }

        private Tuple<int, int> UpdateViewRectangle(Grid grid, Rectangle viewRect, MouseEventArgs e)
        {
            var rowSpan = Grid.GetRowSpan(viewRect);
            var row = GetGridRow(e, grid);
            while (row + rowSpan / 2 > grid.RowDefinitions.Count)
            {
                row--;
            }

            var colSpan = Grid.GetColumnSpan(viewRect);
            var col = GetGridColumn(e, grid);
            while (col + colSpan / 2 > grid.ColumnDefinitions.Count)
            {
                col--;
            }

            Grid.SetRow(viewRect, Math.Max(row - rowSpan / 2, 0));
            Grid.SetColumn(viewRect, Math.Max(col - colSpan / 2, 0));

            return Tuple.Create(row, col);
        }

        private Tuple<int, int> UpdateViewRectangle(Grid grid, Rectangle viewRect, int row, int column)
        {
            var rowSpan = Grid.GetRowSpan(viewRect);
            var columnSpan = Grid.GetColumnSpan(viewRect);

            Grid.SetRow(viewRect, Math.Max(row - rowSpan / 2, 0));
            Grid.SetColumn(viewRect, Math.Max(column - columnSpan / 2, 0));

            return Tuple.Create(row, column);
        }

        private int GetGridRow(MouseEventArgs e, Grid grid)
        {
            double y = e.GetPosition(grid).Y;
            double start = 0.0;
            int row = 0;
            foreach (RowDefinition rd in grid.RowDefinitions)
            {
                start += rd.ActualHeight;
                if (y < start)
                    break;

                row++;
            }

            return row;
        }

        private int GetGridColumn(MouseEventArgs e, Grid grid)
        {
            double x = e.GetPosition(grid).X;
            double start = 0.0;
            int column = 0;
            foreach (ColumnDefinition rd in grid.ColumnDefinitions)
            {
                start += rd.ActualWidth;
                if (x < start)
                    break;

                column++;
            }

            return column;
        }

        private int GetFirstRow()
        {
            return Grid.GetRow(SrcViewRectangle);
        }

        private int GetFirstColumn()
        {
            return Grid.GetColumn(SrcViewRectangle);
        }

        private void ApplyViewRectToGrids()
        {
            var row = GetFirstRow();
            var col = GetFirstColumn();

            SrcDataGrid.Scroll(row, col, row, col);
            DstDataGrid.Scroll(row, col, row, col);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            diffConfig = FindConfig();
            ExecuteDiff();

            SrcDataGrid.ScrolledModelRows += SrcDataGrid_Scrolled;
            SrcDataGrid.ScrolledModelColumns += SrcDataGrid_Scrolled;
            DstDataGrid.ScrolledModelRows += DstDataGrid_Scrolled;
            DstDataGrid.ScrolledModelColumns += DstDataGrid_Scrolled;
        }

        private ExcelSheetDiffConfig FindConfig()
        {
            var config = new ExcelSheetDiffConfig();

            return config;
        }

        private void SrcLocationGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var indices = UpdateViewRectangle(SrcLocationGrid, SrcViewRectangle, e);
            UpdateViewRectangle(DstLocationGrid, DstViewRectangle, indices.Item1, indices.Item2);

            ApplyViewRectToGrids();
        }

        private void SrcLocationGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var indices = UpdateViewRectangle(SrcLocationGrid, SrcViewRectangle, e);
                UpdateViewRectangle(DstLocationGrid, DstViewRectangle, indices.Item1, indices.Item2);

                updateViewRectangleEnabled = false;
                ApplyViewRectToGrids();
                updateViewRectangleEnabled = true;
            }
        }

        private void DstLocationGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var indices = UpdateViewRectangle(DstLocationGrid, DstViewRectangle, e);
            UpdateViewRectangle(SrcLocationGrid, SrcViewRectangle, indices.Item1, indices.Item2);

            ApplyViewRectToGrids();
        }

        private void DstLocationGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var indices = UpdateViewRectangle(DstLocationGrid, DstViewRectangle, e);
                UpdateViewRectangle(SrcLocationGrid, SrcViewRectangle, indices.Item1, indices.Item2);

                updateViewRectangleEnabled = false;
                ApplyViewRectToGrids();
                updateViewRectangleEnabled = true;
            }
        }

        private void SrcDataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateViewRectangle(SrcViewRectangle, SrcDataGrid);
            UpdateViewRectangle(DstViewRectangle, DstDataGrid);
        }

        private void DstDataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateViewRectangle(SrcViewRectangle, SrcDataGrid);
            UpdateViewRectangle(DstViewRectangle, DstDataGrid);
        }

        private void LocationSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!UpdateLocationGridDefinitions(SrcLocationGrid, SrcDataGrid, e.NewSize))
                return;

            if (!UpdateLocationGridDefinitions(DstLocationGrid, DstDataGrid, e.NewSize))
                return;

            UpdateViewRectangle(SrcViewRectangle, SrcDataGrid);
            UpdateViewRectangle(DstViewRectangle, DstDataGrid);

            UpdateLocationGridColors(SrcLocationGrid, SrcDataGrid);
            UpdateLocationGridColors(DstLocationGrid, DstDataGrid);
        }

        private void RemoveRectangles(Grid grid)
        {
            int intTotalChildren = grid.Children.Count - 1;
            for (int intCounter = intTotalChildren; intCounter > 0; intCounter--)
            {
                if (grid.Children[intCounter].GetType() == typeof(Rectangle))
                {
                    var ucCurrentChild = (Rectangle)grid.Children[intCounter];
                    grid.Children.Remove(ucCurrentChild);
                }
            }
        }

        private static bool EqualColor(Color? lhs, Color? rhs)
        {
            if (!lhs.HasValue && !rhs.HasValue)
                return true;

            return lhs.Equals(rhs);
        }

        private void UpdateLocationGridColors(Grid locationGrid, FastGridControl dataGrid)
        {
            RemoveRectangles(locationGrid);

            var columns = Enumerable.Range(0, locationGrid.ColumnDefinitions.Count).ToList();
            for (int i = 0; i < locationGrid.RowDefinitions.Count; i++)
            {
                var splitedCells = columns.Select(c => dataGrid.Model.GetCell(dataGrid, i, c).BackgroundColor)
                    .SplitByComparison((c, n) => EqualColor(c, n));

                var startPosition = 0;
                foreach (var sc in splitedCells)
                {
                    var color = sc.First();
                    if (!color.HasValue)
                    {
                        startPosition += sc.Count();
                        continue;
                    }

                    var rectangle = new Rectangle();
                    rectangle.Fill = new SolidColorBrush(color.Value);

                    Grid.SetRow(rectangle, i);
                    Grid.SetRowSpan(rectangle, 1);
                    for (int k = 1; locationGrid.RowDefinitions[i].Height.Value * k < minimumLocationGridSize; k++)
                        Grid.SetRowSpan(rectangle, k);

                    Grid.SetColumn(rectangle, startPosition);
                    Grid.SetColumnSpan(rectangle, sc.Count());
                    startPosition += sc.Count();
                    for (int k = sc.Count(); locationGrid.ColumnDefinitions[0].Width.Value * k < minimumLocationGridSize; k++)
                    {
                        Grid.SetColumnSpan(rectangle, k);
                    }

                    Grid.SetZIndex(rectangle, 0);

                    locationGrid.Children.Add(rectangle);
                }
            }
        }

        private void SrcLocationGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            LocationGrid_MouseWheel(e);
        }

        private void DstLocationGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            LocationGrid_MouseWheel(e);
        }

        private void LocationGrid_MouseWheel(MouseWheelEventArgs e)
        {
            if (SrcDataGrid.Model == null || DstDataGrid.Model == null)
                return;

            var srcRowSpan = Grid.GetRowSpan(SrcViewRectangle);
            var dstRowSpan = Grid.GetRowSpan(DstViewRectangle);

            var srcRow = Math.Max((Grid.GetRow(SrcViewRectangle) - e.Delta / 10), 0);
            srcRow = Math.Min(srcRow, Math.Max(SrcLocationGrid.RowDefinitions.Count - srcRowSpan, 0));
            Grid.SetRow(SrcViewRectangle, srcRow);

            var dstRow = Math.Max((Grid.GetRow(SrcViewRectangle) - e.Delta / 10), 0);
            dstRow = Math.Min(dstRow, Math.Max(DstLocationGrid.RowDefinitions.Count - dstRowSpan, 0));
            Grid.SetRow(DstViewRectangle, dstRow);

            ApplyViewRectToGrids();
        }

        private void SrcValueTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ValueTextBox_GotFocus();
        }

        private void DstValueTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ValueTextBox_GotFocus();
        }

        private void ValueTextBox_GotFocus()
        {
            var margin = 10;
            var srcTextHeight = CalculateTextBoxHeight(SrcValueTextBox) + margin;
            var dstTextHeight = CalculateTextBoxHeight(DstValueTextBox) + margin;

            var height = Math.Max(srcTextHeight, dstTextHeight);
            height = Math.Max(height, SrcValueTextBox.Height);
            height = Math.Min(height, SrcDataGrid.ActualHeight);

            SrcValueTextBox.Height = height;
            DstValueTextBox.Height = height;
        }

        private void SrcValueTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ValueTextBox_LostFocus();
        }

        private void DstValueTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ValueTextBox_LostFocus();
        }

        private void ValueTextBox_LostFocus()
        {
            SrcValueTextBox.Height = valueTextBoxHeight;
            DstValueTextBox.Height = valueTextBoxHeight;
        }

        private double CalculateTextBoxHeight(RichTextBox rtb)
        {
            var text = string.Join("", rtb.Document.Blocks.First().ContentStart.Paragraph.Inlines.Select(i => new TextRange(i.ContentStart, i.ContentEnd).Text));
            var height = Math.Ceiling(SrcValueTextBox.FontSize * SrcValueTextBox.FontFamily.LineSpacing);

            return text.Split('\n').Count() * height;
        }

        private void SrcDataGrid_SelectedCellsChanged(object sender, FastWpfGrid.SelectionChangedEventArgs e)
        {
            DataGrid_SelectedCellsChanged(SrcDataGrid, DstDataGrid);
        }

        private void DstDataGrid_SelectedCellsChanged(object sender, FastWpfGrid.SelectionChangedEventArgs e)
        {
            DataGrid_SelectedCellsChanged(DstDataGrid, SrcDataGrid);
        }

        private void DataGrid_SelectedCellsChanged(FastGridControl changed, FastGridControl other)
        {
            var model = changed?.Model as DiffGridModel;
            if (model == null)
                return;

            var otherModel = other?.Model as DiffGridModel;
            if (otherModel == null)
                return;

            var currentCell = changed.CurrentCell;
            if (currentCell.Row == null || currentCell.Column == null || !currentCell.Row.HasValue || !currentCell.Column.HasValue)
                return;

            if (other.CurrentCell != currentCell)
            {
                other.CurrentCell = new FastGridCellAddress(currentCell.Row, currentCell.Column);
                return;
            }

            var updated = false;
            changed.GetSelectedModelCells().ToList().ForEach(c => updated |= other.AddSelectedCell(c));
            if (updated)
                DstDataGrid.InvalidateAll();

            var srcValue = (SrcDataGrid.Model as DiffGridModel).GetCellText(SrcDataGrid.CurrentCell.Row.Value, SrcDataGrid.CurrentCell.Column.Value);
            var dstValue = (DstDataGrid.Model as DiffGridModel).GetCellText(DstDataGrid.CurrentCell.Row.Value, DstDataGrid.CurrentCell.Column.Value);

            UpdateValueDiff(srcValue, dstValue);
        }

        private string GetRichTextString(RichTextBox textBox)
        {
            var textRange = new TextRange(textBox.Document.ContentStart, textBox.Document.ContentEnd);

            return textRange.Text;
        }

        private IEnumerable<DiffResult<string>> DiffCellValue(IEnumerable<string> src, IEnumerable<string> dst)
        {
            var option = DiffOption<string>.Default;
            option.Order = DiffOrder.LazyDeleteFirst;

            return DiffUtil.OptimizeCaseDeletedFirst(DiffUtil.Diff(src, dst, option));
        }

        private string ConvertWhiteSpaces(string str)
        {
            return new string(str.Select(c =>
            {
                if (Encoding.UTF8.GetByteCount(c.ToString()) == 1)
                    return ' ';
                else
                    return '　';

            }).ToArray());
        }

        private string ConvertWhiteSpaces(char c)
        {
            if (Encoding.UTF8.GetByteCount(c.ToString()) == 1)
                return " ";
            else
                return "　";
        }

        private void DiffModifiedLine(IEnumerable<DiffResult<char>> results, List<Tuple<string, Color?>> ranges, bool isSrc)
        {
            var splited = results.SplitByComparison((c, n) => c.Status.Equals(n.Status)).ToList();

            foreach (var sr in splited)
            {
                var status = sr.First().Status;
                if (status == DiffStatus.Equal)
                {
                    ranges.Add(Tuple.Create<string, Color?>(new string(sr.Select(r => r.Obj1).ToArray()), null));
                }
                else if (status == DiffStatus.Modified)
                {
                    var str = new string(sr.Select(r => isSrc ? r.Obj1 : r.Obj2).ToArray());
                    ranges.Add(Tuple.Create<string, Color?>(str, EMColor.LightOrange));
                }
                else if (status == DiffStatus.Deleted)
                {
                    var str = new string(sr.Select(r => r.Obj1).ToArray());
                    ranges.Add(Tuple.Create<string, Color?>(str, EMColor.LightGray));
                }
                else if (status == DiffStatus.Inserted)
                {
                    var str = new string(sr.Select(r => r.Obj2).ToArray());
                    ranges.Add(Tuple.Create<string, Color?>(str, EMColor.Orange));
                }
            }

            ranges.Add(Tuple.Create<string, Color?>("\n", null));
        }

        private void DiffEqualLine(DiffResult<string> lineDiffResult, List<Tuple<string, Color?>> ranges)
        {
            ranges.Add(Tuple.Create<string, Color?>(lineDiffResult.Obj1, null));
            ranges.Add(Tuple.Create<string, Color?>("\n", null));
        }

        private void DiffDeletedLine(DiffResult<string> lineDiffResult, List<Tuple<string, Color?>> ranges, bool isSrc)
        {
            var str = isSrc ? lineDiffResult.Obj1 : ConvertWhiteSpaces(lineDiffResult.Obj1.ToString());
            ranges.Add(Tuple.Create<string, Color?>(str, isSrc ? EMColor.LightGray : EMColor.LightGray));
            ranges.Add(Tuple.Create<string, Color?>("\n", null));
        }

        private void DiffInsertedLine(DiffResult<string> lineDiffResult, List<Tuple<string, Color?>> ranges, bool isSrc)
        {
            var str = isSrc ? ConvertWhiteSpaces(lineDiffResult.Obj2) : lineDiffResult.Obj2;
            ranges.Add(Tuple.Create<string, Color?>(str, isSrc ? EMColor.LightGray : EMColor.Orange));
            ranges.Add(Tuple.Create<string, Color?>("\n", null));
        }

        private void UpdateValueDiff(string srcValue, string dstValue)
        {
            SrcValueTextBox.Document.Blocks.First().ContentStart.Paragraph.Inlines.Clear();
            DstValueTextBox.Document.Blocks.First().ContentStart.Paragraph.Inlines.Clear();

            var srcLines = srcValue.Split('\n').Select(s => s.TrimEnd());
            var dstLines = dstValue.Split('\n').Select(s => s.TrimEnd());

            var lineDiffResults = DiffCellValue(srcLines, dstLines).ToList();

            var srcRange = new List<Tuple<string, Color?>>();
            var dstRange = new List<Tuple<string, Color?>>();
            foreach (var lineDiffResult in lineDiffResults)
            {
                if (lineDiffResult.Status == DiffStatus.Equal)
                {
                    DiffEqualLine(lineDiffResult, srcRange);
                    DiffEqualLine(lineDiffResult, dstRange);
                }
                else if (lineDiffResult.Status == DiffStatus.Modified)
                {
                    var opt = DiffOption<char>.Default;
                    opt.Order = DiffOrder.LazyDeleteFirst;
                    var charDiffResults = DiffUtil.OptimizeCaseDeletedFirst(DiffUtil.Diff(lineDiffResult.Obj1, lineDiffResult.Obj2, opt)).ToList();

                    DiffModifiedLine(charDiffResults.Where(r => r.Status != DiffStatus.Inserted), srcRange, true);
                    DiffModifiedLine(charDiffResults.Where(r => r.Status != DiffStatus.Deleted), dstRange, false);
                }
                else if (lineDiffResult.Status == DiffStatus.Deleted)
                {
                    DiffDeletedLine(lineDiffResult, srcRange, true);
                    DiffDeletedLine(lineDiffResult, dstRange, false);
                }
                else if (lineDiffResult.Status == DiffStatus.Inserted)
                {
                    DiffInsertedLine(lineDiffResult, srcRange, true);
                    DiffInsertedLine(lineDiffResult, dstRange, false);
                }
            }

            foreach (var r in srcRange)
            {
                var bc = r.Item2.HasValue ? new SolidColorBrush(r.Item2.Value) : new SolidColorBrush();
                SrcValueTextBox.Document.Blocks.First().ContentStart.Paragraph.Inlines.Add(new Run(r.Item1) { Background = bc });
            }

            foreach (var r in dstRange)
            {
                var bc = r.Item2.HasValue ? new SolidColorBrush(r.Item2.Value) : new SolidColorBrush();
                DstValueTextBox.Document.Blocks.First().ContentStart.Paragraph.Inlines.Add(new Run(r.Item1) { Background = bc });
            }
        }

        private static void UpdateTextColor(RichTextBox rtb, int offset, int length, Color color)
        {
            var textRange = GetTextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd, offset, length);
            if (textRange == null)
                return;

            textRange.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(color));
        }

        private static TextRange GetTextRange(TextPointer position, TextPointer endPosition, int offset, int length)
        {
            TextPointer contentsStart = position;
            TextPointer start = null;
            while (position != null)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text || position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.None)
                {
                    if (start == null)
                    {
                        for (int i = 0, count = position.GetTextRunLength(LogicalDirection.Forward); i <= count; ++i)
                        {
                            if (start == null)
                            {
                                start = position.GetPositionAtOffset(i);
                                TextRange range = new TextRange(contentsStart, start);
                                if (range.Text.Length >= offset)
                                {
                                    position = start;
                                }
                                else
                                {
                                    start = null;
                                }
                            }
                            else
                            {
                                var end = position.GetPositionAtOffset(i);
                                if (end != null)
                                {
                                    TextRange range = new TextRange(start, end);
                                    if (range.Text.Length >= length)
                                        return range;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0, count = position.GetTextRunLength(LogicalDirection.Forward); i <= count; ++i)
                        {
                            var end = position.GetPositionAtOffset(i);
                            TextRange range = new TextRange(start, end);
                            if (range.Text.Length >= length)
                                return range;
                        }
                    }
                }

                position = position.GetNextInsertionPosition(LogicalDirection.Forward);
            }

            return null;
        }

        private void DiffButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteDiff();
        }

        private ExcelSheetReadConfig CreateReadConfig()
        {
            var setting = ((App)Application.Current).Setting;

            return new ExcelSheetReadConfig()
            {
                SkipFirstBlankRows = setting.SkipFirstBlankRows,
            };
        }

        private void ExecuteDiff()
        {
            if (!File.Exists(SrcPathTextBox.Text) || !File.Exists(DstPathTextBox.Text))
                return;

            SrcDataGrid.ScrollIntoView(FastGridCellAddress.Empty);
            DstDataGrid.ScrollIntoView(FastGridCellAddress.Empty);

            SrcDataGrid.FirstVisibleColumnScrollIndex = 0;
            SrcDataGrid.FirstVisibleRowScrollIndex = 0;
            DstDataGrid.FirstVisibleColumnScrollIndex = 0;
            DstDataGrid.FirstVisibleRowScrollIndex = 0;

            SrcDataGrid.InitializeComponent();
            DstDataGrid.InitializeComponent();

            var config = CreateReadConfig();
            var wb1 = ExcelWorkbook.Create(SrcPathTextBox.Text, config);
            var wb2 = ExcelWorkbook.Create(DstPathTextBox.Text, config);

            var tmpSrcSheetIndex = diffConfig.SrcSheetIndex;
            var tmpDstSheetIndex = diffConfig.DstSheetIndex;

            SrcSheetCombobox.Items.Clear();
            DstSheetCombobox.Items.Clear();

            wb1.Sheets.Keys.ToList().ForEach(s => SrcSheetCombobox.Items.Add(s));
            wb2.Sheets.Keys.ToList().ForEach(s => DstSheetCombobox.Items.Add(s));

            SrcSheetCombobox.SelectedIndex = Math.Max(tmpSrcSheetIndex, 0);
            DstSheetCombobox.SelectedIndex = Math.Max(tmpDstSheetIndex, 0);

            var sheet1 = wb1.Sheets[SrcSheetCombobox.SelectedItem.ToString()];
            var sheet2 = wb2.Sheets[DstSheetCombobox.SelectedItem.ToString()];

            var diff = ExcelSheet.Diff(sheet1, sheet2, diffConfig);

            var modelConfig = new DiffGridModelConfig();
            var srcModel = new DiffGridModel(DiffType.Source, diff, modelConfig);
            var dstModel = new DiffGridModel(DiffType.Dest, diff, modelConfig);

            SrcDataGrid.Model = srcModel;
            DstDataGrid.Model = dstModel;

            UpdateLocationGridDefinitions(SrcLocationGrid, SrcDataGrid, SrcLocationGrid.RenderSize);
            UpdateLocationGridDefinitions(DstLocationGrid, DstDataGrid, DstLocationGrid.RenderSize);

            UpdateViewRectangle(SrcViewRectangle, SrcDataGrid);
            UpdateViewRectangle(DstViewRectangle, DstDataGrid);

            UpdateLocationGridColors(SrcLocationGrid, SrcDataGrid);
            UpdateLocationGridColors(DstLocationGrid, DstDataGrid);

            if (!App.Instance.KeepFileHistory)
                App.Instance.UpdateRecentFiles(SrcPathTextBox.Text, DstPathTextBox.Text);
        }

        private void FreezeSrcColumn_Click(object sender, RoutedEventArgs e)
        {
            if (SrcDataGrid.CurrentCell != null)
            {
                (SrcDataGrid.Model as DiffGridModel).FreezeColumn(SrcDataGrid.CurrentCell.Column);
                SrcDataGrid.NotifyColumnArrangeChanged();

                (DstDataGrid.Model as DiffGridModel).FreezeColumn(SrcDataGrid.CurrentCell.Column);
                DstDataGrid.NotifyColumnArrangeChanged();
            }
        }

        private void FreezeDstColumn_Click(object sender, RoutedEventArgs e)
        {
            if (DstDataGrid.CurrentCell != null)
            {
                (SrcDataGrid.Model as DiffGridModel).FreezeColumn(DstDataGrid.CurrentCell.Column);
                SrcDataGrid.NotifyColumnArrangeChanged();

                (DstDataGrid.Model as DiffGridModel).FreezeColumn(DstDataGrid.CurrentCell.Column);
                DstDataGrid.NotifyColumnArrangeChanged();
            }
        }

        private void UnfreezeSrcColumn_Click(object sender, RoutedEventArgs e)
        {
            (SrcDataGrid.Model as DiffGridModel).UnfreezeColumn();
            SrcDataGrid.NotifyColumnArrangeChanged();

            (DstDataGrid.Model as DiffGridModel).UnfreezeColumn();
            DstDataGrid.NotifyColumnArrangeChanged();
        }

        private void UnfreezeDstColumn_Click(object sender, RoutedEventArgs e)
        {
            (SrcDataGrid.Model as DiffGridModel).UnfreezeColumn();
            SrcDataGrid.NotifyColumnArrangeChanged();

            (DstDataGrid.Model as DiffGridModel).UnfreezeColumn();
            DstDataGrid.NotifyColumnArrangeChanged();
        }

        private void SetSrcHeader_Click(object sender, RoutedEventArgs e)
        {
            (SrcDataGrid.Model as DiffGridModel).SetHeader(SrcDataGrid.CurrentCell.Row);
            SrcDataGrid.NotifyRefresh();

            (DstDataGrid.Model as DiffGridModel).SetHeader(SrcDataGrid.CurrentCell.Row);
            DstDataGrid.NotifyRefresh();
        }

        private void SetDstHeader_Click(object sender, RoutedEventArgs e)
        {
            (DstDataGrid.Model as DiffGridModel).SetHeader(DstDataGrid.CurrentCell.Row);
            DstDataGrid.NotifyRefresh();

            (SrcDataGrid.Model as DiffGridModel).SetHeader(SrcDataGrid.CurrentCell.Row);
            SrcDataGrid.NotifyRefresh();
        }

        private void ResetSrcHeader_Click(object sender, RoutedEventArgs e)
        {
            (DstDataGrid.Model as DiffGridModel).SetHeader(0);
            DstDataGrid.NotifyRefresh();

            (SrcDataGrid.Model as DiffGridModel).SetHeader(0);
            SrcDataGrid.NotifyRefresh();
        }

        private void ResetDstHeader_Click(object sender, RoutedEventArgs e)
        {
            (DstDataGrid.Model as DiffGridModel).SetHeader(0);
            DstDataGrid.NotifyRefresh();

            (SrcDataGrid.Model as DiffGridModel).SetHeader(0);
            SrcDataGrid.NotifyRefresh();
        }

        private void SwapButton_Click(object sender, RoutedEventArgs e)
        {
            Swap();
        }

        private void Swap()
        {
            var srcTmp = SrcSheetCombobox.SelectedIndex;
            var dstTmp = DstSheetCombobox.SelectedIndex;

            var tmp = SrcPathTextBox.Text;
            SrcPathTextBox.Text = DstPathTextBox.Text;
            DstPathTextBox.Text = tmp;

            diffConfig.SrcSheetIndex = dstTmp;
            diffConfig.DstSheetIndex = srcTmp;

            ExecuteDiff();
        }

        private void SrcPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SrcSheetCombobox.Items.Clear();
        }

        private void DstPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            DstSheetCombobox.Items.Clear();
        }

        private void SrcSheetCombobox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (diffConfig == null)
                return;

            diffConfig.SrcSheetIndex = Math.Max(SrcSheetCombobox.SelectedIndex, 0);
        }

        private void DstSheetCombobox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (diffConfig == null)
                return;

            diffConfig.DstSheetIndex = Math.Max(DstSheetCombobox.SelectedIndex, 0);
        }

        private void DiffByHeaderSrc_Click(object sender, RoutedEventArgs e)
        {
            var headerIndex = SrcDataGrid.CurrentCell.Row.HasValue ? SrcDataGrid.CurrentCell.Row.Value : -1;

            diffConfig.HeaderIndex = headerIndex;

            ExecuteDiff();
        }

        private void DiffByHeaderDst_Click(object sender, RoutedEventArgs e)
        {
            var headerIndex = DstDataGrid.CurrentCell.Row.HasValue ? DstDataGrid.CurrentCell.Row.Value : -1;

            diffConfig.HeaderIndex = headerIndex;

            ExecuteDiff();
        }
    }
}
