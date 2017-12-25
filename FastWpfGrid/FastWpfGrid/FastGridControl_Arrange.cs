using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FastWpfGrid
{
    partial class FastGridControl
    {
        public int FirstVisibleColumnScrollIndex;
        public int FirstVisibleRowScrollIndex;
        private int _modelRowCount;
        private int _modelColumnCount;
        private int _realRowCount;
        private int _realColumnCount;
        //private int _frozenRowCount;
        //private int _frozenColumnCount;

        private SeriesSizes _rowSizes = new SeriesSizes();
        private SeriesSizes _columnSizes = new SeriesSizes();

        public int VisibleRowCount
        {
            get { return _rowSizes.GetVisibleScrollCount(FirstVisibleRowScrollIndex, GridScrollAreaHeight); }
        }

        public int VisibleColumnCount
        {
            get { return _columnSizes.GetVisibleScrollCount(FirstVisibleColumnScrollIndex, GridScrollAreaWidth); }
        }

        public bool IsWide
        {
            get { return _realColumnCount > WideColumnsLimit; }
        }

        public bool FlexibleRows
        {
            get { return !IsWide && AllowFlexibleRows; }
        }

        private int GetRowTop(int row)
        {
            if (row < _rowSizes.FrozenCount) return _rowSizes.GetFrozenPosition(row) + HeaderHeight;
            return _rowSizes.GetSizeSum(FirstVisibleRowScrollIndex, row - _rowSizes.FrozenCount) + HeaderHeight + FrozenHeight;
            //return (row - FirstVisibleRow) * RowHeight + HeaderHeight;
        }

        public void SetMaxRowSize(int size)
        {
            _rowSizes.MaxSize = size;
        }

        public void SetMaxColumnSize(int size)
        {
            _columnSizes.MaxSize = size;
        }

        public void SetMinRowSize(int size)
        {
            _rowSizes.MinSize = size;
        }

        public void SetMinColumnSize(int size)
        {
            _columnSizes.MinSize = size;
        }

        private int GetColumnLeft(int column)
        {
            if (column < _columnSizes.FrozenCount) return _columnSizes.GetFrozenPosition(column) + HeaderWidth;
            return _columnSizes.GetSizeSum(FirstVisibleColumnScrollIndex, column - _columnSizes.FrozenCount) + HeaderWidth + FrozenWidth;
            //return (column - FirstVisibleColumn) * ColumnWidth + HeaderWidth;
        }

        private IntRect GetCellRect(int row, int column)
        {
            return new IntRect(new IntPoint(GetColumnLeft(column), GetRowTop(row)), new IntSize(_columnSizes.GetSizeByRealIndex(column) + 1, _rowSizes.GetSizeByRealIndex(row) + 1));
        }

        private IntRect GetContentRect(IntRect rect)
        {
            return rect.GrowSymmetrical(-CellPaddingHorizontal, -CellPaddingVertical);
        }

        private IntRect GetRowHeaderRect(int row)
        {
            return new IntRect(new IntPoint(0, GetRowTop(row)), new IntSize(HeaderWidth + 1, _rowSizes.GetSizeByRealIndex(row) + 1));
        }

        private IntRect GetColumnHeaderRect(int column)
        {
            return new IntRect(new IntPoint(GetColumnLeft(column), 0), new IntSize(_columnSizes.GetSizeByRealIndex(column) + 1, HeaderHeight + 1));
        }

        private IntRect GetColumnHeadersScrollRect()
        {
            return new IntRect(new IntPoint(HeaderWidth + FrozenWidth, 0), new IntSize(GridScrollAreaWidth, HeaderHeight + 1));
        }

        private IntRect GetRowHeadersScrollRect()
        {
            return new IntRect(new IntPoint(0, HeaderHeight + FrozenHeight), new IntSize(HeaderWidth + 1, GridScrollAreaHeight));
        }

        private IntRect GetFrozenColumnsRect()
        {
            return new IntRect(new IntPoint(HeaderWidth, HeaderHeight), new IntSize(_columnSizes.FrozenSize + 1, GridScrollAreaHeight));
        }

        private IntRect GetFrozenRowsRect()
        {
            return new IntRect(new IntPoint(HeaderWidth, HeaderHeight), new IntSize(GridScrollAreaHeight, _rowSizes.FrozenSize + 1));
        }

        public Rect GetColumnHeaderRectangle(int modelColumnIndex)
        {
            var rect = (IsTransposed ? GetRowHeaderRect(_rowSizes.ModelToReal(modelColumnIndex)) : GetColumnHeaderRect(_columnSizes.ModelToReal(modelColumnIndex))).ToRect();
            var pt = image.PointToScreen(rect.TopLeft);
            return new Rect(pt, rect.Size);
        }

        public int? GetResizingColumn(Point pt)
        {
            if (pt.Y > HeaderHeight) return null;

            int frozenWidth = FrozenWidth;
            if ((int) pt.X - HeaderWidth <= frozenWidth + ColumnResizeTheresold)
            {
                if ((int) pt.X - HeaderWidth >= frozenWidth - ColumnResizeTheresold && (int) pt.X - HeaderWidth <= FrozenWidth - ColumnResizeTheresold)
                {
                    return _columnSizes.FrozenCount - 1;
                }
                int index = _columnSizes.GetFrozenIndexOnPosition((int) pt.X - HeaderWidth);
                int begin = _columnSizes.GetPositionByRealIndex(index) + HeaderWidth;
                int end = begin + _columnSizes.GetSizeByRealIndex(index);
                if (pt.X >= begin - ColumnResizeTheresold && pt.X <= begin + ColumnResizeTheresold) return index - 1;
                if (pt.X >= end - ColumnResizeTheresold && pt.X <= end + ColumnResizeTheresold) return index;
            }
            else
            {
                int scrollXStart = _columnSizes.GetPositionByScrollIndex(FirstVisibleColumnScrollIndex);
                int index = _columnSizes.GetScrollIndexOnPosition((int) pt.X - HeaderWidth - frozenWidth + scrollXStart);
                int begin = _columnSizes.GetPositionByScrollIndex(index) + HeaderWidth + frozenWidth - scrollXStart;
                int end = begin + _columnSizes.GetSizeByScrollIndex(index);
                if (pt.X >= begin - ColumnResizeTheresold && pt.X <= begin + ColumnResizeTheresold) return index - 1 + _columnSizes.FrozenCount;
                if (pt.X >= end - ColumnResizeTheresold && pt.X <= end + ColumnResizeTheresold) return index + _columnSizes.FrozenCount;
            }
            return null;
        }

        private int? GetSeriesIndexOnPosition(double position, int headerSize, SeriesSizes series, int firstVisible)
        {
            if (position <= headerSize) return null;
            int frozenSize = series.FrozenSize;
            if (position <= headerSize + frozenSize) return series.GetFrozenIndexOnPosition((int) Math.Round(position - headerSize));
            return series.GetScrollIndexOnPosition(
                (int) Math.Round(position - headerSize - frozenSize) + series.GetPositionByScrollIndex(firstVisible)
                       ) + series.FrozenCount;
        }

        public FastGridCellAddress GetCellAddress(Point pt)
        {
            if (pt.X <= HeaderWidth && pt.Y < HeaderHeight)
            {
                return FastGridCellAddress.GridHeader;
            }
            if (pt.X >= GridScrollAreaWidth + HeaderWidth + FrozenWidth)
            {
                return FastGridCellAddress.Empty;
            }
            if (pt.Y >= GridScrollAreaHeight + HeaderHeight + FrozenHeight)
            {
                return FastGridCellAddress.Empty;
            }

            int? row = GetSeriesIndexOnPosition(pt.Y, HeaderHeight, _rowSizes, FirstVisibleRowScrollIndex);
            int? col = GetSeriesIndexOnPosition(pt.X, HeaderWidth, _columnSizes, FirstVisibleColumnScrollIndex);

            return new FastGridCellAddress(row, col);
        }

        public void ScrollCurrentCellIntoView()
        {
            ScrollIntoView(_currentCell);
        }

        public void ScrollModelIntoView(FastGridCellAddress cell)
        {
            ScrollIntoView(ModelToReal(cell));
        }

        public void ScrollIntoView(FastGridCellAddress cell)
        {
            if (cell.Row.HasValue)
            {
                if (cell.Row.Value >= _rowSizes.FrozenCount)
                {
                    int newRow = _rowSizes.ScrollInView(FirstVisibleRowScrollIndex, cell.Row.Value - _rowSizes.FrozenCount, GridScrollAreaHeight);
                    ScrollContent(newRow, FirstVisibleColumnScrollIndex);
                }
            }

            if (cell.Column.HasValue)
            {
                if (cell.Column.Value >= _columnSizes.FrozenCount)
                {
                    int newColumn = _columnSizes.ScrollInView(FirstVisibleColumnScrollIndex, cell.Column.Value - _columnSizes.FrozenCount, GridScrollAreaWidth);
                    ScrollContent(FirstVisibleRowScrollIndex, newColumn);
                }
            }

            AdjustInlineEditorPosition();
            AdjustSelectionMenuPosition();
            AdjustScrollBarPositions();
        }

        public FastGridCellAddress CurrentCell
        {
            get { return _currentCell; }
            set { MoveCurrentCell(value.Row, value.Column); }
        }

        public int? CurrentRow
        {
            get { return _currentCell.IsCell ? _currentCell.Row : null; }
            set { CurrentCell = _currentCell.ChangeRow(value); }
        }

        public int? CurrentColumn
        {
            get { return _currentCell.IsCell ? _currentCell.Column : null; }
            set { CurrentCell = _currentCell.ChangeColumn(value); }
        }

        public void NotifyColumnArrangeChanged()
        {
            UpdateSeriesCounts();
            FixCurrentCellAndSetSelectionToCurrentCell();
            AdjustScrollbars();
            SetScrollbarMargin();
            FixScrollPosition();
            InvalidateAll();
        }

        public void NotifyRowArrangeChanged()
        {
            UpdateSeriesCounts();
            FixCurrentCellAndSetSelectionToCurrentCell();
            AdjustScrollbars();
            SetScrollbarMargin();
            FixScrollPosition();
            InvalidateAll();
        }

        private void UpdateSeriesCounts()
        {
            _rowSizes.Count = IsTransposed ? _modelColumnCount : _modelRowCount;
            _columnSizes.Count = IsTransposed ? _modelRowCount : _modelColumnCount;

            if (_model != null)
            {
                if (IsTransposed)
                {
                    _columnSizes.SetExtraordinaryIndexes(_model.GetHiddenRows(this), _model.GetFrozenRows(this));
                    _rowSizes.SetExtraordinaryIndexes(_model.GetHiddenColumns(this), _model.GetFrozenColumns(this));
                }
                else
                {
                    _rowSizes.SetExtraordinaryIndexes(_model.GetHiddenRows(this), _model.GetFrozenRows(this));
                    _columnSizes.SetExtraordinaryIndexes(_model.GetHiddenColumns(this), _model.GetFrozenColumns(this));
                }
            }

            _realRowCount = _rowSizes.RealCount;
            _realColumnCount = _columnSizes.RealCount;
        }

        private static void Exchange<T>(ref T a, ref T b)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }

        private void OnIsTransposedPropertyChanged()
        {
            if (_isTransposed != IsTransposed)
            {
                _selectedRealRowCountLimitLoaded = false;
                _selectedRealColumnCountLimitLoaded = false;
                _isTransposed = IsTransposed;
                Exchange(ref FirstVisibleColumnScrollIndex, ref FirstVisibleRowScrollIndex);
                if (_currentCell.IsCell) _currentCell = new FastGridCellAddress(_currentCell.Column, _currentCell.Row);
                var oldSelected = _selectedCells.ToList();
                ClearSelectedCells();
                foreach (var cell in oldSelected) AddSelectedCell(new FastGridCellAddress(cell.Column, cell.Row, cell.IsColumnHeader));
                _mouseOverCell = FastGridCellAddress.Empty;
                _mouseOverRow = null;
                _mouseOverRowHeader = null;
                _mouseOverColumnHeader = null;
                UpdateSeriesCounts();
                RecountColumnWidths();
                RecountRowHeights();
                RecalculateDefaultCellSize();
                AdjustScrollbars();
                AdjustScrollBarPositions();
                AdjustInlineEditorPosition();
                AdjustSelectionMenuPosition();
            }
        }

        public int HeaderHeight
        {
            get { return _headerHeight; }
            set
            {
                _headerHeight = value;
                SetScrollbarMargin();
            }
        }

        public int HeaderWidth
        {
            get { return _headerWidth; }
            set
            {
                _headerWidth = value;
                SetScrollbarMargin();
            }
        }

        private void SetScrollbarMargin()
        {
            vscroll.Margin = new Thickness
                {
                    Top = HeaderHeight + FrozenHeight,
                };
            hscroll.Margin = new Thickness
                {
                    Left = HeaderWidth + FrozenWidth,
                };
        }

        public int FrozenWidth
        {
            get { return _columnSizes.FrozenSize; }
        }

        public int FrozenHeight
        {
            get { return _rowSizes.FrozenSize; }
        }

        private IntRect GetScrollRect()
        {
            return new IntRect(new IntPoint(HeaderWidth + FrozenWidth, HeaderHeight + FrozenHeight), new IntSize(GridScrollAreaWidth, GridScrollAreaHeight));
        }

        private IntRect GetGridHeaderRect()
        {
            return new IntRect(new IntPoint(0, 0), new IntSize(HeaderWidth + 1, HeaderHeight + 1));
        }

        private FastGridCellAddress RealToModel(FastGridCellAddress address)
        {
            if (IsTransposed)
            {
                return new FastGridCellAddress(
                    address.Column.HasValue ? _columnSizes.RealToModel(address.Column.Value) : (int?)null,
                    address.Row.HasValue ? _rowSizes.RealToModel(address.Row.Value) : (int?)null,
                    address.IsGridHeader
                    );
            }
            else
            {
                return new FastGridCellAddress(
                    address.Row.HasValue ? _rowSizes.RealToModel(address.Row.Value) : (int?)null,
                    address.Column.HasValue ? _columnSizes.RealToModel(address.Column.Value) : (int?)null,
                    address.IsGridHeader
                    );

            }
        }

        private FastGridCellAddress ModelToReal(FastGridCellAddress address)
        {
            if (IsTransposed)
            {
                return new FastGridCellAddress(
                    address.Column.HasValue ? _rowSizes.ModelToReal(address.Column.Value) : (int?)null,
                    address.Row.HasValue ? _columnSizes.ModelToReal(address.Row.Value) : (int?)null,
                    address.IsGridHeader
                    );
            }
            else
            {
                return new FastGridCellAddress(
                    address.Row.HasValue ? _rowSizes.ModelToReal(address.Row.Value) : (int?)null,
                    address.Column.HasValue ? _columnSizes.ModelToReal(address.Column.Value) : (int?)null,
                    address.IsGridHeader
                    );

            }
        }


        private void OnAllowFlexibleRowsPropertyChanged()
        {
            RecountRowHeights();
            RecountColumnWidths();
            AdjustScrollbars();
            AdjustScrollBarPositions();
            AdjustInlineEditorPosition();
            AdjustSelectionMenuPosition();
            InvalidateAll();
        }

        private ActiveSeries GetActiveRealRows()
        {
            var res = new ActiveSeries();
            int visibleRows = VisibleRowCount;
            for (int i = FirstVisibleRowScrollIndex; i < FirstVisibleRowScrollIndex + visibleRows; i++)
            {
                int model = _rowSizes.RealToModel(i + _rowSizes.FrozenCount);
                res.ScrollVisible.Add(model);
            }
            for (int i = 0; i < _rowSizes.FrozenCount; i++)
            {
                int model = _rowSizes.RealToModel(i);
                res.Frozen.Add(model);
            }
            foreach (var cell in _selectedCells)
            {
                if (!cell.Row.HasValue) continue;
                int model = _rowSizes.RealToModel(cell.Row.Value);
                res.Selected.Add(model);
            }
            return res;
        }

        private ActiveSeries GetActiveRealColumns()
        {
            var res = new ActiveSeries();
            int visibleCols = VisibleColumnCount;
            for (int i = FirstVisibleColumnScrollIndex; i < FirstVisibleColumnScrollIndex + visibleCols; i++)
            {
                int model = _columnSizes.RealToModel(i + _columnSizes.FrozenCount);
                res.ScrollVisible.Add(model);
            }
            for (int i = 0; i < _columnSizes.FrozenCount; i++)
            {
                int model = _columnSizes.RealToModel(i);
                res.Frozen.Add(model);
            }
            foreach(var cell in _selectedCells)
            {
                if (!cell.Column.HasValue) continue;
                int model = _columnSizes.RealToModel(cell.Column.Value);
                res.Selected.Add(model);
            }
            return res;
        }

        public ActiveSeries GetActiveRows()
        {
            return IsTransposed ? GetActiveRealColumns() : GetActiveRealRows();
        }

        public ActiveSeries GetActiveColumns()
        {
            return IsTransposed ? GetActiveRealRows() : GetActiveRealColumns();
        }

        private void SetExtraordinaryRealColumns()
        {
            if (_model != null)
            {
                if (IsTransposed)
                {
                    _columnSizes.SetExtraordinaryIndexes(_model.GetHiddenRows(this), _model.GetFrozenRows(this));
                }
                else
                {
                    _columnSizes.SetExtraordinaryIndexes(_model.GetHiddenColumns(this), _model.GetFrozenColumns(this));
                }
            }
        }

        private void RecountColumnWidths()
        {
            _columnSizes.Clear();

            SetExtraordinaryRealColumns();

            if (_drawBuffer == null) return;
            if (GridScrollAreaWidth > 16)
            {
                if (_columnSizes.MaxSize.HasValue && GridScrollAreaWidth - 16 < _columnSizes.MaxSize)
                    _columnSizes.MaxSize = GridScrollAreaWidth - 16;
            }

            if (IsWide) return;
            if (_model == null) return;
            int rowCount = _isTransposed ? _modelColumnCount : _modelRowCount;
            int colCount = _isTransposed ? _modelRowCount : _modelColumnCount;

            for (int col = 0; col < colCount; col++)
            {
                var cell = _isTransposed ? _model.GetRowHeader(this, col) : _model.GetColumnHeader(this, col);
                _columnSizes.PutSizeOverride(col, GetCellContentWidth(cell) + 2 * CellPaddingHorizontal);
            }

            int visRows = VisibleRowCount;
            int row0 = FirstVisibleRowScrollIndex + _rowSizes.FrozenCount;
            for (int row = row0; row < Math.Min(row0 + visRows, rowCount); row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    var cell = _isTransposed ? _model.GetCell(this, col, row) : _model.GetCell(this, row, col);
                    _columnSizes.PutSizeOverride(col, GetCellContentWidth(cell, _columnSizes.MaxSize) + 2 * CellPaddingHorizontal);
                }
            }

            _columnSizes.BuildIndex();
        }

        private void SetExtraordinaryRealRows()
        {
            if (_model != null)
            {
                if (IsTransposed)
                {
                    _rowSizes.SetExtraordinaryIndexes(_model.GetHiddenColumns(this), _model.GetFrozenColumns(this));
                }
                else
                {
                    _rowSizes.SetExtraordinaryIndexes(_model.GetHiddenRows(this), _model.GetFrozenRows(this));
                }
            }
        }

        private void RecountRowHeights()
        {
            _rowSizes.Clear();
            SetExtraordinaryRealRows();
            if (_drawBuffer == null) return;
            if (GridScrollAreaHeight > 16)
            {
                if (_rowSizes.MaxSize.HasValue && GridScrollAreaHeight - 16 < _rowSizes.MaxSize.Value)
                    _rowSizes.MaxSize = GridScrollAreaHeight - 16;
            }

            CountVisibleRowHeights();
        }

        private bool CountVisibleRowHeights()
        {
            if (!FlexibleRows) return false;
            int colCount = _isTransposed ? _modelRowCount : _modelColumnCount;
            int rowCount = VisibleRowCount;
            bool changed = false;
            for (int row = FirstVisibleRowScrollIndex; row < FirstVisibleRowScrollIndex + rowCount; row++)
            {
                int modelRow = _rowSizes.RealToModel(row);
                if (_rowSizes.HasSizeOverride(modelRow)) continue;
                changed = true;
                for (int col = 0; col < colCount; col++)
                {
                    var cell = _isTransposed ? GetModelCell(col, row) : GetModelCell(row, col);
                    var cellContentHeight = Math.Min(GetCellContentHeight(cell), _rowSizes.MaxSize.Value);

                    if (cellContentHeight > _rowSizes.DefaultSize)
                        _rowSizes.PutSizeOverride(modelRow, cellContentHeight + 2 * CellPaddingVertical + 2 + RowHeightReserve);
                }
            }
            _rowSizes.BuildIndex();
            //AdjustVerticalScrollBarRange();
            return changed;
        }

        private void FixScrollPosition()
        {
            if (FirstVisibleRowScrollIndex >= vscroll.Maximum) FirstVisibleRowScrollIndex = (int) vscroll.Maximum;
            if (FirstVisibleColumnScrollIndex >= hscroll.Maximum) FirstVisibleColumnScrollIndex = (int) hscroll.Maximum;
            ClearSelectedCells();
            if (_currentCell.Row.HasValue)
            {
                if (_currentCell.Row >= _realRowCount)
                    _currentCell = _currentCell.ChangeRow(_realRowCount > 0 ? _realRowCount - 1 : (int?) null);
            }
            if (_currentCell.Column.HasValue)
            {
                if (_currentCell.Column >= _realColumnCount)
                    _currentCell = _currentCell.ChangeColumn(_realColumnCount > 0 ? _realColumnCount - 1 : (int?) null);
            }
            if (_currentCell.IsCell) AddSelectedCell(_currentCell);
            AdjustScrollBarPositions();
            OnChangeSelectedCells(false);
        }

        //public int FirstVisibleRowModelIndex
        //{
        //    get
        //    {
        //        if (IsTransposed) return -1;
        //        return _rowSizes.RealToModel(FirstVisibleRowScrollIndex + _rowSizes.FrozenCount);
        //    }
        //}
    }
}
