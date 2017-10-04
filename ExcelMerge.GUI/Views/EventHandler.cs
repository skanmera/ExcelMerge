using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Input;
using FastWpfGrid;
using Microsoft.Practices.Unity;
using ExcelMerge.GUI.Extensions;
using ExcelMerge.GUI.Models;

namespace ExcelMerge.GUI.Views
{
    class EventHandler : IDataGridEventHandler, ILocationGridEventHandler, IViewportEventListener, IValueTextBoxEventListener
    {
        public string Key { get; }

        public EventHandler(string key)
        {
            Key = key;
        }

        public void OnScrolled(FastGridControl target, IUnityContainer container)
        {
            var dataGrid = container.Resolve<FastGridControl>(Key);

            if (dataGrid == null || dataGrid == target)
                return;

            SyncScroll(target, dataGrid);
            RecalculateViewport(container.Resolve<Rectangle>(Key), dataGrid);
        }

        private void SyncScroll(FastGridControl src, FastGridControl dst)
        {
            dst.Scroll(src.FirstVisibleRowScrollIndex, src.FirstVisibleColumnScrollIndex, src.VerticalScrollBarOffset, src.HorizontalScrollBarOffset);
            dst.NotifyRefresh();
        }

        private void RecalculateViewport(Rectangle viewport, FastGridControl dataGrid)
        {
            if (dataGrid.VisibleColumnCount <= 0 || dataGrid.VisibleRowCount <= 0)
                return;

            Grid.SetColumn(viewport, dataGrid.FirstVisibleColumnScrollIndex);
            Grid.SetColumnSpan(viewport, dataGrid.VisibleColumnCount);
            Grid.SetRow(viewport, dataGrid.FirstVisibleRowScrollIndex);
            Grid.SetRowSpan(viewport, dataGrid.VisibleRowCount);
        }

        public void OnSizeChanged(FastGridControl target, IUnityContainer container, SizeChangedEventArgs e)
        {
            var dataGrid = container.Resolve<FastGridControl>(Key);

            if (dataGrid == null || dataGrid != target)
                return;

            RecalculateViewport(container.Resolve<Rectangle>(Key), dataGrid);
        }

        public void OnModelUpdated(FastGridControl target, IUnityContainer container)
        {
            var dataGrid = container.Resolve<FastGridControl>(Key);
            if (target.Model == null || target != dataGrid)
                return;

            var locationGrid = container.Resolve<Grid>(Key);
            if (locationGrid == null)
                return;

            var rowCount = target.Model.RowCount;
            var colCount = target.Model.ColumnCount;
            UpdateLocationGridDefinisions(locationGrid, locationGrid.RenderSize, rowCount, colCount);

            RecalculateViewport(container.Resolve<Rectangle>(Key), target);

            var colorMap = CreateColorMap(target);
            UpdateLocationGridColors(locationGrid, colorMap);
        }

        private void UpdateLocationGridDefinisions(Grid grid, Size newGridSize, int rowCount, int columnCount)
        {
            var width = columnCount > 0 ? newGridSize.Width / columnCount : 0;
            var height = rowCount > 0 ? newGridSize.Height / rowCount : 0;

            grid.ColumnDefinitions.Clear();
            for (int i = 0; i < columnCount; i++)
            {
                var colDef = new ColumnDefinition()
                {
                    Width = new GridLength(width, GridUnitType.Star),
                };

                grid.ColumnDefinitions.Add(colDef);
            }

            grid.RowDefinitions.Clear();
            for (int i = 0; i < rowCount; i++)
            {
                var rowDef = new RowDefinition()
                {
                    Height = new GridLength(height, GridUnitType.Star),
                };

                grid.RowDefinitions.Add(rowDef);
            }
        }

        private void UpdateLocationGridColors(Grid locationGrid, Dictionary<int, Dictionary<int, Color?>> colorMap)
        {
            locationGrid.ClearChildren<Rectangle>();

            if (!locationGrid.RowDefinitions.Any() || !locationGrid.ColumnDefinitions.Any())
                return;

            var rowHeight = locationGrid.RowDefinitions[0].Height.Value;
            var columnWidth = locationGrid.ColumnDefinitions[0].Width.Value;

            foreach (var rowColors in colorMap)
            {
                var rowIndex = rowColors.Key; var columnColorMap = rowColors.Value;

                var sections = columnColorMap.SplitByComparison((c, n) => EqualColor(c.Value, n.Value));
                foreach (var section in sections)
                {
                    var color = section.First().Value;
                    if (!color.HasValue)
                        continue;

                    var colSpan = (int)Math.Max(section.Last().Key - section.First().Key + 1, 5d / columnWidth);
                    var rowSpan = (int)Math.Max(1, 5d / rowHeight);

                    var rectangle = new Rectangle();
                    rectangle.Fill = new SolidColorBrush(color.Value);

                    Grid.SetRow(rectangle, rowIndex);
                    Grid.SetRowSpan(rectangle, rowSpan);
                    Grid.SetColumn(rectangle, section.First().Key);
                    Grid.SetColumnSpan(rectangle, colSpan);
                    Grid.SetZIndex(rectangle, 0);

                    locationGrid.Children.Add(rectangle);
                }
            }
        }

        private static Dictionary<int, Dictionary<int, Color?>> CreateColorMap(FastGridControl dataGrid)
        {
            var ret = new Dictionary<int, Dictionary<int, Color?>>();

            if (dataGrid.Model == null)
                return ret;

            var hiddenRows = dataGrid.Model.GetHiddenRows(dataGrid).ToList();

            for (int i = 0, rcount = dataGrid.Model.RowCount; i < rcount; i++)
            {
                if (hiddenRows.Remove(i))
                    continue;

                var columnColorMap = new Dictionary<int, Color?>();
                for (int j = 0, ccount = dataGrid.Model.ColumnCount; j < ccount; j++)
                {
                    columnColorMap.Add(j, dataGrid.Model.GetCell(dataGrid, i, j)?.BackgroundColor);
                }

                ret.Add(i, columnColorMap);
            }

            return ret;
        }

        private static bool EqualColor(Color? lhs, Color? rhs)
        {
            if (!lhs.HasValue && !rhs.HasValue)
                return true;

            return lhs.Equals(rhs);
        }

        public void OnMouseDown(Grid target, IUnityContainer container, MouseEventArgs e)
        {
            if (target != container.Resolve<Grid>(Key))
                return;

            var viewport = container.Resolve<Rectangle>(Key);

            var rowSpan = Grid.GetRowSpan(viewport);
            var currentRow = Grid.GetRow(viewport);
            var row = target.GetRow(e);
            while (row + rowSpan / 2 > target.RowDefinitions.Count)
                row--;
            Grid.SetRow(viewport, Math.Max(row - rowSpan / 2, 0));

            var colSpan = Grid.GetColumnSpan(viewport);
            var currentCol = Grid.GetColumn(viewport);
            var col = target.GetColumn(e);
            while (col + colSpan / 2 > target.ColumnDefinitions.Count)
                col--;
            Grid.SetColumn(viewport, Math.Max(col - colSpan / 2, 0));

            if (currentRow != row || currentCol != col)
                ViewportEventDispatcher.DispatchMoveEvent(viewport, container);
        }

        public void OnMouseWheel(Grid target, IUnityContainer container, MouseWheelEventArgs e)
        {
            if (target != container.Resolve<Grid>(Key))
                return;

            var viewport = container.Resolve<Rectangle>(Key);

            var rowSpan = Grid.GetRowSpan(viewport);
            var currentRow = Grid.GetRow(viewport);
            var row = currentRow - (Math.Sign(e.Delta) * rowSpan / 2);
            var last = Math.Max((target.RowDefinitions.Count) - Grid.GetRowSpan(viewport), 0);
            row = Math.Max(row, 0);
            row = Math.Min(row, last);

            Grid.SetRow(viewport, row);

            if (currentRow != row)
                ViewportEventDispatcher.DispatchMoveEvent(viewport, container);
        }

        public void OnSizeChanged(Grid target, IUnityContainer container, SizeChangedEventArgs e) { }

        public void OnViewportMoved(Rectangle target, IUnityContainer container)
        {
            var row = Grid.GetRow(target);
            var col = Grid.GetColumn(target);

            container.Resolve<FastGridControl>(Key).Scroll(row, col, row, col);
        }

        public void OnSelectedCellChanged(FastGridControl target, IUnityContainer container)
        {
            var dataGrid = container.Resolve<FastGridControl>(Key);
            if (target == dataGrid)
                return;

            var targetCell = new FastGridCellAddress(target.CurrentCell.Row, target.CurrentCell.Column);
            if (!targetCell.Equals(dataGrid.CurrentCell))
            {
                dataGrid.CurrentCell = targetCell;

                if (target.GetSelectedModelCells().Aggregate(false, (r, c) => r |= dataGrid.AddSelectedCell(c)))
                {
                    dataGrid.InvalidateAll();
                }
            }

            var valuteTextBox = container.ResolveAll<RichTextBox>();
        }

        private double CalculateTextBoxHeight(RichTextBox rtb)
        {
            var text = string.Join("", rtb.Document.Blocks.First().ContentStart.Paragraph.Inlines.Select(i => new TextRange(i.ContentStart, i.ContentEnd).Text));
            var height = Math.Ceiling(rtb.FontSize * rtb.FontFamily.LineSpacing);

            return text.Split('\n').Count() * height;
        }

        public void OnGotFocus(RichTextBox textBox, IUnityContainer container)
        {
            var margin = 10;
            var textHeightList = container.ResolveAll<RichTextBox>().Select(rtb => CalculateTextBoxHeight(rtb) + margin);

            var height = textHeightList.Max();

            container.ResolveAll<RichTextBox>().ForEach(rtb => rtb.Height = height);
        }

        public void OnLostFocus(RichTextBox textBox, IUnityContainer container)
        {
            container.ResolveAll<RichTextBox>().ForEach(rtb => rtb.Height = double.NaN);
        }

        public void OnHeaderChanged(FastGridControl target, IUnityContainer container)
        {
            var dataGrid = container.Resolve<FastGridControl>(Key);
            (dataGrid.Model as DiffGridModel).SetHeader(target.CurrentCell.Row);

            dataGrid.NotifyRowArrangeChanged();
        }

        public void OnHeaderReset(FastGridControl target, IUnityContainer container)
        {
            var dataGrid = container.Resolve<FastGridControl>(Key);
            (dataGrid.Model as DiffGridModel).SetHeader(0);

            dataGrid.NotifyRowArrangeChanged();
        }

        public void OnFrozenColumnChanged(FastGridControl target, IUnityContainer container)
        {
            var dataGrid = container.Resolve<FastGridControl>(Key);
            (dataGrid.Model as DiffGridModel).FreezeColumn(target.CurrentCell.Column);

            dataGrid.NotifyColumnArrangeChanged();
        }

        public void OnFrozenColumnReset(FastGridControl target, IUnityContainer container)
        {
            var dataGrid = container.Resolve<FastGridControl>(Key);
            (dataGrid.Model as DiffGridModel).UnfreezeColumn();

            dataGrid.NotifyColumnArrangeChanged();
        }
    }
}
