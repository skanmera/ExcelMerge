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
using SKCore.Collection;
using SKCore.Wpf.Controls.Utilities;
using ExcelMerge.GUI.Models;
using ExcelMerge.GUI.Settings;

namespace ExcelMerge.GUI.Views
{
    class EventHandler : IDataGridEventHandler, ILocationGridEventHandler, IViewportEventListener, IValueTextBoxEventListener
    {
        public string Key { get; }

        public EventHandler(string key)
        {
            Key = key;
        }

        public void OnParentLoaded(IUnityContainer container)
        {
            foreach (var grid in container.ResolveAll<FastGridControl>())
            {
                grid.ScrolledModelRows += (sender, e) => DataGridEventDispatcher.DispatchScrollEvnet(grid, container);
                grid.ScrolledModelColumns += (sender, e) => DataGridEventDispatcher.DispatchScrollEvnet(grid, container);
            }
        }

        public void OnPreExecuteDiff(IUnityContainer container)
        {
            foreach (var grid in container.ResolveAll<FastGridControl>())
            {
                grid.Model = null;
                grid.ScrollIntoView(FastGridCellAddress.Empty);
                grid.FirstVisibleColumnScrollIndex = 0;
                grid.FirstVisibleRowScrollIndex = 0;
                grid.InitializeComponent();
            }

            DataGridEventDispatcher.DispatchApplicationSettingUpdateEvent(container);
        }

        public void OnPostExecuteDiff(IUnityContainer container) { }

        public void OnFileSettingUpdated(FastGridControl target, IUnityContainer container, FileSetting fileSetting)
        {
            if (target != container.Resolve<FastGridControl>(Key))
                return;

            if (fileSetting != null)
            {
                var model = target.Model as DiffGridModel;
                if (model != null)
                {
                    model.SetColumnHeader(fileSetting.ColumnHeaderIndex);
                    if (string.IsNullOrEmpty(fileSetting.RowHeaderName))
                        model.SetRowHeader(fileSetting.RowHeaderIndex);
                    else
                        model.SetRowHeader(fileSetting.RowHeaderName);
                }

                target.MaxRowHeaderWidth = fileSetting.MaxRowHeaderWidth;
            }
        }

        public void OnApplicationSettingUpdated(IUnityContainer container)
        {
            foreach (var grid in container.ResolveAll<FastGridControl>())
            {
                grid.AlternatingColors = App.Instance.Setting.AlternatingColors;
                grid.CellFontName = App.Instance.Setting.FontName;
                grid.SetMaxColumnSize(App.Instance.Setting.CellWidth);
                grid.SetMinColumnSize(App.Instance.Setting.CellWidth);

                DataGridEventDispatcher.DispatchModelUpdateEvent(grid, container);
            }
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
            //dst.NotifyRefresh();
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

            var rowCount = target.Model.RowCount - target.Model.GetHiddenRows(target).Count;
            var colCount = target.Model.ColumnCount;
            UpdateLocationGridDefinisions(locationGrid, locationGrid.RenderSize, rowCount, colCount);

            var viewport = container.Resolve<Rectangle>(Key);
            RecalculateViewport(viewport, target);

            var colorMap = CreateColorMap(target);
            UpdateLocationGridColors(locationGrid, colorMap, viewport);

            target.NotifyRefresh();
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

        private void UpdateLocationGridColors(Grid locationGrid, List<Dictionary<int, Color?>> colorMaps, Rectangle viewport)
        {
            locationGrid.ClearChildren<Rectangle>(new List<UIElement> { viewport });

            if (!locationGrid.RowDefinitions.Any() || !locationGrid.ColumnDefinitions.Any())
                return;

            var rowHeight = locationGrid.RowDefinitions[0].Height.Value;
            var columnWidth = locationGrid.ColumnDefinitions[0].Width.Value;

            var rowIndex = 0;
            foreach (var columnColorMap in colorMaps)
            {
                var sections = columnColorMap.SplitByRegularity((items, current) => EqualColor(items.Last().Value, current.Value));
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

                rowIndex++;
            }
        }

        private static List<Dictionary<int, Color?>> CreateColorMap(FastGridControl dataGrid)
        {
            var ret = new List<Dictionary<int, Color?>>();

            if (dataGrid.Model == null)
                return ret;

            var model = dataGrid.Model as DiffGridModel;
            var rowCount = model.RowCount - model.GetHiddenRows(dataGrid).Count;
            for (int i = 0; i < rowCount; i++)
            {
                var columnColorMap = new Dictionary<int, Color?>();
                for (int j = 0, ccount = dataGrid.Model.ColumnCount; j < ccount; j++)
                {
                    columnColorMap.Add(j, model.GetCell(dataGrid, i, j, true)?.BackgroundColor);
                }

                ret.Add(columnColorMap);
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

            var row = target.GetRow(e.GetPosition(target));
            if (!row.HasValue)
                return;

            while (row + rowSpan / 2 > target.RowDefinitions.Count)
                row--;

            Grid.SetRow(viewport, Math.Max(row.Value - rowSpan / 2, 0));

            var colSpan = Grid.GetColumnSpan(viewport);
            var currentCol = Grid.GetColumn(viewport);

            var col = target.GetColumn(e.GetPosition(target));
            if (!col.HasValue)
                return;

            while (col + colSpan / 2 > target.ColumnDefinitions.Count)
                col--;

            Grid.SetColumn(viewport, Math.Max(col.Value - colSpan / 2, 0));

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
                if (dataGrid.Model == null)
                    return;

                if (dataGrid.Model.RowCount < targetCell.Row || dataGrid.Model.ColumnCount < targetCell.Column)
                    return;

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

            var height = Math.Min(textHeightList.Max(), App.Instance.MainWindow.Height / 2);

            foreach (var rtb in container.ResolveAll<RichTextBox>())
                rtb.Height = height;
        }

        public void OnLostFocus(RichTextBox textBox, IUnityContainer container)
        {
            foreach (var rtb in container.ResolveAll<RichTextBox>())
                rtb.Height = 30d;
        }

        public void OnColumnHeaderChanged(FastGridControl target, IUnityContainer container)
        {
            var dataGrid = container.Resolve<FastGridControl>(Key);
            (dataGrid.Model as DiffGridModel).SetColumnHeader(target.CurrentCell.Row);

            dataGrid.NotifyRowArrangeChanged();
        }

        public void OnColumnHeaderReset(FastGridControl target, IUnityContainer container)
        {
            var dataGrid = container.Resolve<FastGridControl>(Key);
            (dataGrid.Model as DiffGridModel).SetColumnHeader(0);

            dataGrid.NotifyRowArrangeChanged();
        }

        public void OnRowHeaderChanged(FastGridControl target, IUnityContainer container)
        {
            var dataGrid = container.Resolve<FastGridControl>(Key);
            (dataGrid.Model as DiffGridModel).SetRowHeader(target.CurrentCell.Column);

            dataGrid.NotifyColumnArrangeChanged();
            DataGridEventDispatcher.DispatchModelUpdateEvent(dataGrid, container);
        }

        public void OnRowHeaderReset(FastGridControl target, IUnityContainer container)
        {
            var dataGrid = container.Resolve<FastGridControl>(Key);
            (dataGrid.Model as DiffGridModel).SetRowHeader(-1);

            dataGrid.NotifyColumnArrangeChanged();
            DataGridEventDispatcher.DispatchModelUpdateEvent(dataGrid, container);
        }

        public void OnScrolled(RichTextBox target, IUnityContainer container, ScrollChangedEventArgs e)
        {
            foreach (var rtb in container.ResolveAll<RichTextBox>())
            {
                rtb.ScrollToVerticalOffset(e.VerticalOffset);
                rtb.ScrollToHorizontalOffset(e.HorizontalOffset);
            }
        }

        public void OnDiffDisplayFormatChanged(IUnityContainer container, bool onlyDiff)
        {
            var grid = container.Resolve<FastGridControl>(Key);

            if (onlyDiff)
                (grid.Model as DiffGridModel)?.HideEqualRows();
            else
                (grid.Model as DiffGridModel)?.ShowEqualRows();

            DataGridEventDispatcher.DispatchModelUpdateEvent(grid, container);
        }
    }
}
