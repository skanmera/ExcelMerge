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
    class DiffViewEventHandler : IDataGridEventListener, ILocationGridEventListener, IViewportEventListener, IValueTextBoxEventListener
    {
        public string Key { get; }

        public DiffViewEventHandler(string key)
        {
            Key = key;
        }

        #region DataGrid

        public void OnParentLoaded(DiffViewEventArgs<FastGridControl> e)
        {
            foreach (var grid in e.Container.ResolveAll<FastGridControl>())
            {
                var a = new DiffViewEventArgs<FastGridControl>(grid, e.Container);

                grid.ScrolledModelRows += (sender, args)
                    => DataGridEventDispatcher.Instance.DispatchScrollEvnet(a);
                grid.ScrolledModelColumns += (sender, args)
                    => DataGridEventDispatcher.Instance.DispatchScrollEvnet(a);
                grid.ColumnWidthChanged += (sender, args)
                    => DataGridEventDispatcher.Instance.DispatchColumnWidthChangeEvent(a, args);
                grid.HoverRowChanged += (sender, args)
                    => DataGridEventDispatcher.Instance.DispatchHoverRowChangeEvent(a, args);
            }
        }

        public void OnPreExecuteDiff(DiffViewEventArgs<FastGridControl> e)
        {
            foreach (var grid in e.Container.ResolveAll<FastGridControl>())
            {
                grid.Model = null;
                grid.ScrollIntoView(FastGridCellAddress.Empty);
                grid.FirstVisibleColumnScrollIndex = 0;
                grid.FirstVisibleRowScrollIndex = 0;
                grid.InitializeComponent();
            }

            DataGridEventDispatcher.Instance.DispatchApplicationSettingUpdateEvent(e);
        }

        public void OnPostExecuteDiff(DiffViewEventArgs<FastGridControl> e)
        {
            //SyncRowHeight(e.Container);
        }

        public void OnFileSettingUpdated(DiffViewEventArgs<FastGridControl> e, FileSetting fileSetting)
        {
            if (e.Sender != e.Container.Resolve<FastGridControl>(Key))
                return;

            if (fileSetting != null)
            {
                var model = e.Sender.Model as DiffGridModel;
                if (model != null)
                {
                    model.SetColumnHeader(fileSetting.ColumnHeaderIndex);
                    if (string.IsNullOrEmpty(fileSetting.RowHeaderName))
                        model.SetRowHeader(fileSetting.RowHeaderIndex);
                    else
                        model.SetRowHeader(fileSetting.RowHeaderName);
                }

                e.Sender.MaxRowHeaderWidth = fileSetting.MaxRowHeaderWidth;
            }
        }

        public void OnApplicationSettingUpdated(DiffViewEventArgs<FastGridControl> e)
        {
            foreach (var grid in e.Container.ResolveAll<FastGridControl>())
            {
                grid.AlternatingColors = App.Instance.Setting.AlternatingColors;
                grid.CellFontName = App.Instance.Setting.FontName;
                grid.SetMaxColumnSize(App.Instance.Setting.ColumnWidth);
                grid.SetMinColumnSize(App.Instance.Setting.ColumnWidth);
                grid.SetMaxRowSize(App.Instance.Setting.MaxRowHeight);
                grid.AllowFlexibleRows = App.Instance.Setting.FitRowHeight;

                var args = new DiffViewEventArgs<FastGridControl>(grid, e.Container);
                DataGridEventDispatcher.Instance.DispatchModelUpdateEvent(args);
            }

            foreach (var textBox in e.Container.ResolveAll<RichTextBox>())
            {
                var args = new DiffViewEventArgs<RichTextBox>(textBox, e.Container);
                ValueTextBoxEventDispatcher.Instance.DispatchLostFocusEvent(args);
            }

            SyncRowHeight(e.Container);
        }

        public void OnScrolled(DiffViewEventArgs<FastGridControl> e)
        {
            var dataGrid = e.Container.Resolve<FastGridControl>(Key);

            if (dataGrid == null || dataGrid == e.Sender)
                return;

            SyncScroll(e.Sender, dataGrid);
            RecalculateViewport(e.Container.Resolve<Rectangle>(Key), dataGrid);
        }

        public void OnSizeChanged(DiffViewEventArgs<FastGridControl> e, SizeChangedEventArgs se)
        {
            var dataGrid = e.Container.Resolve<FastGridControl>(Key);

            if (dataGrid == null || dataGrid != e.Sender)
                return;

            RecalculateViewport(e.Container.Resolve<Rectangle>(Key), dataGrid);
        }

        public void OnModelUpdated(DiffViewEventArgs<FastGridControl> e)
        {
            var dataGrid = e.Container.Resolve<FastGridControl>(Key);
            if (e.Sender.Model == null || e.Sender != dataGrid)
                return;

            var locationGrid = e.Container.Resolve<Grid>(Key);
            if (locationGrid == null)
                return;

            var rowCount = e.Sender.Model.RowCount - e.Sender.Model.GetHiddenRows(e.Sender).Count;
            var colCount = e.Sender.Model.ColumnCount;
            UpdateLocationGridDefinisions(locationGrid, locationGrid.RenderSize, rowCount, colCount);

            var viewport = e.Container.Resolve<Rectangle>(Key);
            RecalculateViewport(viewport, e.Sender);

            var colorMap = CreateColorMap(e.Sender);
            UpdateLocationGridColors(locationGrid, colorMap, viewport);

            e.Sender.NotifyRefresh();
        }

        public void OnSelectedCellChanged(DiffViewEventArgs<FastGridControl> e)
        {
            var dataGrid = e.Container.Resolve<FastGridControl>(Key);
            if (e.Sender == dataGrid)
                return;

            var targetCell = new FastGridCellAddress(e.Sender.CurrentCell.Row, e.Sender.CurrentCell.Column);
            if (!targetCell.Equals(dataGrid.CurrentCell))
            {
                if (dataGrid.Model == null)
                    return;

                if (dataGrid.Model.RowCount < targetCell.Row || dataGrid.Model.ColumnCount < targetCell.Column)
                    return;

                dataGrid.CurrentCell = targetCell;

                if (e.Sender.GetSelectedModelCells().Aggregate(false, (r, c) => r |= dataGrid.AddSelectedCell(c)))
                {
                    dataGrid.InvalidateAll();
                }
            }

            var valuteTextBox = e.Container.ResolveAll<RichTextBox>();
        }

        public void OnColumnHeaderChanged(DiffViewEventArgs<FastGridControl> e)
        {
            var dataGrid = e.Container.Resolve<FastGridControl>(Key);
            (dataGrid.Model as DiffGridModel).SetColumnHeader(e.Sender.CurrentCell.Row);

            dataGrid.NotifyRowArrangeChanged();
        }

        public void OnColumnHeaderReset(DiffViewEventArgs<FastGridControl> e)
        {
            var dataGrid = e.Container.Resolve<FastGridControl>(Key);
            (dataGrid.Model as DiffGridModel).SetColumnHeader(0);

            dataGrid.NotifyRowArrangeChanged();
        }

        public void OnRowHeaderChanged(DiffViewEventArgs<FastGridControl> e)
        {
            var dataGrid = e.Container.Resolve<FastGridControl>(Key);
            (dataGrid.Model as DiffGridModel).SetRowHeader(e.Sender.CurrentCell.Column);

            dataGrid.NotifyColumnArrangeChanged();
            DataGridEventDispatcher.Instance.DispatchModelUpdateEvent(e);
        }

        public void OnRowHeaderReset(DiffViewEventArgs<FastGridControl> e)
        {
            var dataGrid = e.Container.Resolve<FastGridControl>(Key);
            (dataGrid.Model as DiffGridModel).SetRowHeader(-1);

            dataGrid.NotifyColumnArrangeChanged();
            DataGridEventDispatcher.Instance.DispatchModelUpdateEvent(e);
        }

        public void OnDiffDisplayFormatChanged(DiffViewEventArgs<FastGridControl> e, bool onlyDiff)
        {
            foreach (var grid in e.Container.ResolveAll<FastGridControl>())
            {
                if (onlyDiff)

                    (grid.Model as DiffGridModel)?.HideEqualRows();
                else
                    (grid.Model as DiffGridModel)?.ShowEqualRows();

                var args = new DiffViewEventArgs<FastGridControl>(grid, e.Container);
                DataGridEventDispatcher.Instance.DispatchModelUpdateEvent(args);

                grid.FirstVisibleRowScrollIndex = 0;
            }

            SyncRowHeight(e.Container);
        }

        public void OnColumnWidthChanged(DiffViewEventArgs<FastGridControl> e, ColumnWidthChangedEventArgs ce)
        {
            foreach (var grid in e.Container.ResolveAll<FastGridControl>())
            {
                if (grid == e.Sender)
                    continue;

                grid.ResizeColumn(ce.NewWidth, ce.Column);
            }
        }

        public void OnHoverRowChanged(DiffViewEventArgs<FastGridControl> e, HoverRowChangedEventArgs he)
        {
            foreach (var grid in e.Container.ResolveAll<FastGridControl>())
            {
                if (e.Sender == grid)
                    continue;

                grid.SetHoverRow(he.Cell);
            }
        }

        #endregion

        #region LocationGrid

        public void OnMouseDown(DiffViewEventArgs<Grid> e, MouseEventArgs me)
        {
            if (e.Sender != e.Container.Resolve<Grid>(Key))
                return;

            var viewport = e.Container.Resolve<Rectangle>(Key);

            var rowSpan = Grid.GetRowSpan(viewport);
            var currentRow = Grid.GetRow(viewport);

            var row = e.Sender.GetRow(me.GetPosition(e.Sender));
            if (!row.HasValue)
                return;

            while (row + rowSpan / 2 > e.Sender.RowDefinitions.Count)
                row--;

            Grid.SetRow(viewport, Math.Max(row.Value - rowSpan / 2, 0));

            var colSpan = Grid.GetColumnSpan(viewport);
            var currentCol = Grid.GetColumn(viewport);

            var col = e.Sender.GetColumn(me.GetPosition(e.Sender));
            if (!col.HasValue)
                return;

            while (col + colSpan / 2 > e.Sender.ColumnDefinitions.Count)
                col--;

            Grid.SetColumn(viewport, Math.Max(col.Value - colSpan / 2, 0));

            if (currentRow != row || currentCol != col)
            {
                var args = new DiffViewEventArgs<Rectangle>(viewport, e.Container);
                ViewportEventDispatcher.Instance.DispatchMoveEvent(args);
            }
        }

        public void OnMouseWheel(DiffViewEventArgs<Grid> e, MouseWheelEventArgs me)
        {
            if (e.Sender != e.Container.Resolve<Grid>(Key))
                return;

            var viewport = e.Container.Resolve<Rectangle>(Key);

            var rowSpan = Grid.GetRowSpan(viewport);
            var currentRow = Grid.GetRow(viewport);
            var row = currentRow - (Math.Sign(me.Delta) * rowSpan / 2);
            var last = Math.Max((e.Sender.RowDefinitions.Count) - Grid.GetRowSpan(viewport), 0);
            row = Math.Max(row, 0);
            row = Math.Min(row, last);

            Grid.SetRow(viewport, row);

            if (currentRow != row)
            {
                var args = new DiffViewEventArgs<Rectangle>(viewport, e.Container);
                ViewportEventDispatcher.Instance.DispatchMoveEvent(args);
            }
        }

        public void OnSizeChanged(DiffViewEventArgs<Grid> e, SizeChangedEventArgs me)
        {
        }

        #endregion

        #region Viewport

        public void OnViewportMoved(DiffViewEventArgs<Rectangle> e)
        {
            var row = Grid.GetRow(e.Sender);
            var col = Grid.GetColumn(e.Sender);

            e.Container.Resolve<FastGridControl>(Key).Scroll(row, col, row, col);
        }

        #endregion

        #region ValueTextbox

        public void OnGotFocus(DiffViewEventArgs<RichTextBox> e)
        {
            var margin = 10;
            var textHeightList = e.Container.ResolveAll<RichTextBox>().Select(rtb => CalculateTextBoxHeight(rtb) + margin);

            var height = Math.Min(textHeightList.Max(), App.Instance.MainWindow.Height / 3);

            foreach (var rtb in e.Container.ResolveAll<RichTextBox>())
                rtb.Height = height;
        }

        public void OnLostFocus(DiffViewEventArgs<RichTextBox> e)
        {
            foreach (var rtb in e.Container.ResolveAll<RichTextBox>())
                rtb.Height = 30d;
        }

        public void OnScrolled(DiffViewEventArgs<RichTextBox> e, ScrollChangedEventArgs se)
        {
            foreach (var rtb in e.Container.ResolveAll<RichTextBox>())
            {
                rtb.ScrollToVerticalOffset(se.VerticalOffset);
                rtb.ScrollToHorizontalOffset(se.HorizontalOffset);
            }
        }

        #endregion

        #region

        private void SyncRowHeight(IUnityContainer container)
        {
            var grids = container.ResolveAll<FastGridControl>();
            if (!grids.Any())
                return;

            foreach (var grid in grids)
                grid.NotifyRefresh();

            var maxRowCount = grids.Max(g => g.ModelRowCount);

            for (int i = 0; i < maxRowCount; i++)
            {
                var maxHeight = grids.Max(g => g.CalculateModelRowHeight(i));
                foreach (var grid in grids)
                {
                    if (grid.GetModelRowHeight(i) != maxHeight)
                        grid.PutSizeOverride(i, maxHeight);
                }
            }

            foreach (var grid in grids)
                grid.BuildRowIndex();
        }

        private void SyncScroll(FastGridControl src, FastGridControl dst)
        {
            dst.Scroll(src.FirstVisibleRowScrollIndex, src.FirstVisibleColumnScrollIndex, src.VerticalScrollBarOffset, src.HorizontalScrollBarOffset);
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

        private double CalculateTextBoxHeight(RichTextBox rtb)
        {
            var text = string.Join("", rtb.Document.Blocks.First().ContentStart.Paragraph.Inlines.Select(i => new TextRange(i.ContentStart, i.ContentEnd).Text));
            var height = Math.Ceiling(rtb.FontSize * rtb.FontFamily.LineSpacing);

            return text.Split('\n').Count() * height;
        }

        #endregion
    }
}
