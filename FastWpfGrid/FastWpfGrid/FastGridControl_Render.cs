using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FastWpfGrid
{
    partial class FastGridControl
    {
        private int _scrolledCount = 0;
        private long _scrolledTick = 0;

        private void RenderGrid()
        {
            var start = DateTime.Now;
            if (_drawBuffer == null)
            {
                ClearInvalidation();
                return;
            }
            using (_drawBuffer.GetBitmapContext())
            {
                int colsToRender = VisibleColumnCount;
                int rowsToRender = VisibleRowCount;

                if (_invalidatedCells.Count > 250)
                {
                    _isInvalidatedAll = true;
                }

                if (!_isInvalidated || _isInvalidatedAll)
                {
                    _drawBuffer.Clear(Colors.White);
                }

                if (ShouldDrawGridHeader())
                {
                    RenderGridHeader();
                }

                // render frozen rows
                for (int row = 0; row < _rowSizes.FrozenCount; row++)
                {
                    for (int col = FirstVisibleColumnScrollIndex + _columnSizes.FrozenCount; col < FirstVisibleColumnScrollIndex + _columnSizes.FrozenCount + colsToRender; col++)
                    {
                        if (!ShouldDrawCell(row, col)) continue;
                        RenderCell(row, col);
                    }
                }

                // render frozen columns
                for (int row = FirstVisibleRowScrollIndex + _rowSizes.FrozenCount; row < FirstVisibleRowScrollIndex + _rowSizes.FrozenCount + rowsToRender; row++)
                {
                    for (int col = 0; col < _columnSizes.FrozenCount; col++)
                    {
                        if (!ShouldDrawCell(row, col)) continue;
                        RenderCell(row, col);
                    }
                }

                // render cells
                for (int row = FirstVisibleRowScrollIndex + _rowSizes.FrozenCount; row < FirstVisibleRowScrollIndex + _rowSizes.FrozenCount + rowsToRender; row++)
                {
                    for (int col = FirstVisibleColumnScrollIndex + _columnSizes.FrozenCount; col < FirstVisibleColumnScrollIndex + _columnSizes.FrozenCount + colsToRender; col++)
                    {
                        if (row < 0 || col < 0 || row >= _realRowCount || col >= _realColumnCount) continue;
                        if (!ShouldDrawCell(row, col)) continue;
                        RenderCell(row, col);
                    }
                }

                // render frozen row headers
                for (int row = 0; row < _rowSizes.FrozenCount; row++)
                {
                    if (!ShouldDrawRowHeader(row)) continue;
                    RenderRowHeader(row);
                }

                // render row headers
                for (int row = FirstVisibleRowScrollIndex + _rowSizes.FrozenCount; row < FirstVisibleRowScrollIndex + _rowSizes.FrozenCount + rowsToRender; row++)
                {
                    if (row < 0 || row >= _realRowCount) continue;
                    if (!ShouldDrawRowHeader(row)) continue;
                    RenderRowHeader(row);
                }

                // render frozen column headers
                for (int col = 0; col < _columnSizes.FrozenCount; col++)
                {
                    if (!ShouldDrawColumnHeader(col)) continue;
                    RenderColumnHeader(col);
                }


                // render column headers
                for (int col = FirstVisibleColumnScrollIndex + _columnSizes.FrozenCount; col < FirstVisibleColumnScrollIndex + _columnSizes.FrozenCount + colsToRender; col++)
                {
                    if (col < 0 || col >= _realColumnCount) continue;
                    if (!ShouldDrawColumnHeader(col)) continue;
                    RenderColumnHeader(col);
                }
            }
            if (_isInvalidatedAll)
            {
                Debug.WriteLine("Render full grid: {0} ms", Math.Round((DateTime.Now - start).TotalMilliseconds));
            }
            ClearInvalidation();
        }

        private void RenderGridHeader()
        {
            if (Model == null) return;
            var cell = Model.GetGridHeader(this);
            var rect = GetGridHeaderRect();
            RenderCell(cell, rect, null, HeaderBackground, FastGridCellAddress.GridHeader);
        }

        private void RenderColumnHeader(int col)
        {
            var cell = GetColumnHeader(col);

            Color? selectedBgColor = null;
            if (col == _currentCell.Column) selectedBgColor = HeaderCurrentBackground;

            var rect = GetColumnHeaderRect(col);
            Color? cellBackground = null;
            if (cell != null) cellBackground = cell.BackgroundColor;

            Color? hoverColor = null;
            if (col == _mouseOverColumnHeader) hoverColor = MouseOverRowColor;

            RenderCell(cell, rect, null, hoverColor ?? selectedBgColor ?? cellBackground ?? HeaderBackground, new FastGridCellAddress(null, col));
        }

        private void RenderRowHeader(int row)
        {
            var cell = GetRowHeader(row);

            Color? selectedBgColor = null;
            if (row == _currentCell.Row) selectedBgColor = HeaderCurrentBackground;

            var rect = GetRowHeaderRect(row);
            Color? cellBackground = null;
            if (cell != null) cellBackground = cell.BackgroundColor;

            Color? hoverColor = null;
            if (row == _mouseOverRowHeader) hoverColor = MouseOverRowColor;

            RenderCell(cell, rect, null, hoverColor ?? selectedBgColor ?? cellBackground ?? HeaderBackground, new FastGridCellAddress(row, null));
        }

        private void RenderCell(int row, int col)
        {
            var rect = GetCellRect(row, col);
            var cell = GetCell(row, col);
            Color? selectedBgColor = null;
            Color? selectedTextColor = null;
            Color? hoverRowColor = null;
            if (_currentCell.TestCell(row, col) || _selectedCells.Contains(new FastGridCellAddress(row, col)))
            {
                selectedBgColor = _isLimitedSelection ? LimitedSelectedColor : SelectedColor;
                selectedTextColor = _isLimitedSelection ? LimitedSelectedTextColor : SelectedTextColor;
            }
            if (row == _mouseOverRow)
            {
                hoverRowColor = MouseOverRowColor;
            }


            Color? cellBackground = null;
            if (cell != null) cellBackground = cell.BackgroundColor;

            RenderCell(cell, rect, selectedTextColor, selectedBgColor
                                                      ?? hoverRowColor
                                                      ?? cellBackground
                                                      ?? GetAlternateBackground(row),
                                                      new FastGridCellAddress(row, col));
        }

        private int GetCellContentHeight(IFastGridCell cell)
        {
            if (cell == null) return 0;
            var font = GetFont(false, false);
            int res = font.TextHeight;
            for (int i = 0; i < cell.BlockCount; i++)
            {
                var block = cell.GetBlock(i);
                if (block.BlockType != FastGridBlockType.Text) continue;
                string text = block.TextData;
                if (text == null) continue;
                int hi = font.GetTextHeight(text);
                if (hi > res) res = hi;
            }
            return res;
        }

        private int GetCellContentWidth(IFastGridCell cell, int? maxSize = null)
        {
            if (cell == null) return 0;
            int count = cell.BlockCount;

            int witdh = 0;
            for (int i = 0; i < count; i++)
            {
                var block = cell.GetBlock(i);
                if (block == null) continue;
                if (i > 0) witdh += BlockPadding;

                switch (block.BlockType)
                {
                    case FastGridBlockType.Text:
                        string text = block.TextData;
                        var font = GetFont(block.IsBold, block.IsItalic);
                        witdh += font.GetTextWidth(text, maxSize);
                        break;
                    case FastGridBlockType.Image:
                        witdh += block.ImageWidth;
                        break;
                }

            }
            return witdh;
        }

        private int RenderBlock(int leftPos, int rightPos, Color? selectedTextColor, Color bgColor, IntRect rectContent, IFastGridCellBlock block, FastGridCellAddress cellAddr, bool leftAlign, bool isHoverCell)
        {
            bool renderBlock = true;
            if (block.MouseHoverBehaviour == MouseHoverBehaviours.HideWhenMouseOut && !isHoverCell) renderBlock = false;

            int width = 0, top = 0, height = 0;

            switch (block.BlockType)
            {
                case FastGridBlockType.Text:
                    var font = GetFont(block.IsBold, block.IsItalic);
                    int textHeight = font.GetTextHeight(block.TextData);
                    width = font.GetTextWidth(block.TextData, _columnSizes.MaxSize);
                    height = textHeight;
                    top = rectContent.Top + (int) Math.Round(rectContent.Height/2.0 - textHeight/2.0);
                    break;
                case FastGridBlockType.Image:
                    top = rectContent.Top + (int) Math.Round(rectContent.Height/2.0 - block.ImageHeight/2.0);
                    height = block.ImageHeight;
                    width = block.ImageWidth;
                    break;
            }

            if (renderBlock && block.CommandParameter != null)
            {
                var activeRect = new IntRect(new IntPoint(leftAlign ? leftPos : rightPos - width, top), new IntSize(width, height)).GrowSymmetrical(1, 1);
                var region = new ActiveRegion
                    {
                        CommandParameter = block.CommandParameter,
                        Rect = activeRect,
                        Tooltip = block.ToolTip,
                    };
                CurrentCellActiveRegions.Add(region);
                if (_mouseCursorPoint.HasValue && activeRect.Contains(_mouseCursorPoint.Value))
                {
                    _drawBuffer.FillRectangle(activeRect, ActiveRegionHoverFillColor);
                    CurrentHoverRegion = region;
                }

                bool renderRectangle = true;
                if (block.MouseHoverBehaviour == MouseHoverBehaviours.HideButtonWhenMouseOut && !isHoverCell) renderRectangle = false;

                if (renderRectangle) _drawBuffer.DrawRectangle(activeRect, ActiveRegionFrameColor);
            }

            switch (block.BlockType)
            {
                case FastGridBlockType.Text:
                    if (renderBlock)
                    {
                        var textOrigin = new IntPoint(leftAlign ? leftPos : rightPos - width, top);
                        var font = GetFont(block.IsBold, block.IsItalic);
                        _drawBuffer.DrawString(textOrigin.X, textOrigin.Y, rectContent, selectedTextColor ?? block.FontColor ?? CellFontColor, UseClearType ? bgColor : (Color?) null,
                                               font,
                                               block.TextData);
                    }
                    break;
                case FastGridBlockType.Image:
                    if (renderBlock)
                    {
                        var imgOrigin = new IntPoint(leftAlign ? leftPos : rightPos - block.ImageWidth, top);
                        var image = GetImage(block.ImageSource);
                        _drawBuffer.Blit(new Point(imgOrigin.X, imgOrigin.Y), image.Bitmap, new Rect(0, 0, block.ImageWidth, block.ImageHeight),
                                         image.KeyColor, image.BlendMode);
                    }
                    break;
            }

            return width;
        }

        private void RenderCell(IFastGridCell cell, IntRect rect, Color? selectedTextColor, Color bgColor, FastGridCellAddress cellAddr)
        {
            bool isHoverCell = !cellAddr.IsEmpty && cellAddr == _mouseOverCell;

            if (isHoverCell)
            {
                _mouseOverCellIsTrimmed = false;
                CurrentCellActiveRegions.Clear();
                CurrentHoverRegion = null;
            }

            if (cell == null) return;
            var rectContent = GetContentRect(rect);
            _drawBuffer.DrawRectangle(rect, GridLineColor);
            _drawBuffer.FillRectangle(rect.GrowSymmetrical(-1, -1), bgColor);

            int count = cell.BlockCount;
            int rightCount = cell.RightAlignBlockCount;
            int leftCount = count - rightCount;
            int leftPos = rectContent.Left;
            int rightPos = rectContent.Right;

            for (int i = count - 1; i >= count - rightCount; i--)
            {
                var block = cell.GetBlock(i);
                if (block == null) continue;
                if (i < count - 1) rightPos -= BlockPadding;
                int blockWi = RenderBlock(leftPos, rightPos, selectedTextColor, bgColor, rectContent, block, cellAddr, false, isHoverCell);
                rightPos -= blockWi;
            }

            for (int i = 0; i < leftCount && leftPos < rightPos; i++)
            {
                var block = cell.GetBlock(i);
                if (block == null) continue;
                if (i > 0) leftPos += BlockPadding;
                int blockWi = RenderBlock(leftPos, rightPos, selectedTextColor, bgColor, rectContent, block, cellAddr, true, isHoverCell);
                leftPos += blockWi;
            }
            switch (cell.Decoration)
            {
                case CellDecoration.StrikeOutHorizontal:
                    _drawBuffer.DrawLine(rect.Left, rect.Top + rect.Height/2, rect.Right, rect.Top + rect.Height/2, cell.DecorationColor ?? Colors.Black);
                    break;
            }
            if (isHoverCell)
            {
                _mouseOverCellIsTrimmed = leftPos > rightPos;
            }
        }

        public static ImageHolder GetImage(string source)
        {
            lock (_imageCache)
            {
                if (_imageCache.ContainsKey(source)) return _imageCache[source];
            }

            string packUri = "pack://application:,,,/" + Assembly.GetEntryAssembly().GetName().Name + ";component/" + source.TrimStart('/');
            BitmapImage bmImage = new BitmapImage();
            bmImage.BeginInit();
            bmImage.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            bmImage.UriSource = new Uri(packUri, UriKind.Absolute);
            bmImage.EndInit();
            var wbmp = new WriteableBitmap(bmImage);

            if (wbmp.Format != PixelFormats.Bgra32)
                wbmp = new WriteableBitmap(new FormatConvertedBitmap(wbmp, PixelFormats.Bgra32, null, 0));

            var image = new ImageHolder(wbmp, bmImage);
            lock (_imageCache)
            {
                _imageCache[source] = image;
            }
            return image;
        }

        private void ScrollContent(int row, int column, int wheeledDelta = 0)
        {
            if (row == FirstVisibleRowScrollIndex && column == FirstVisibleColumnScrollIndex)
            {
                if (FirstVisibleRowScrollIndex + 1 == RealRowCount || RealRowCount == 0 || row == 0)
                {
                    var tick = DateTime.Now.Ticks;
                    if (tick - _scrolledTick < 5000000)
                    {
                        if (wheeledDelta > 0)
                        {
                            _scrolledCount--;
                        }
                        else if (wheeledDelta < 0)
                        {
                            _scrolledCount++;
                        }
                        else
                        {
                            if (row == 0)
                            {
                                _scrolledCount--;
                            }
                            else
                            {
                                _scrolledCount++;
                            }
                        }
                    }
                    else
                    {
                        _scrolledCount = 0;
                    }

                    _scrolledTick = tick;

                    if (_scrolledCount > 10)
                    {
                        OnScrolledBeyondEnd();
                        _scrolledCount = 0;
                    }
                    else if (_scrolledCount < -10)
                    {
                        OnScrolledBeyondStart();
                        _scrolledCount = 0;
                    }
                }

                return;
            }

            if (row != FirstVisibleRowScrollIndex && !_isInvalidatedAll && column == FirstVisibleColumnScrollIndex
                && Math.Abs(row - FirstVisibleRowScrollIndex) * 2 < VisibleRowCount)
            {
                using (var ctx = CreateInvalidationContext())
                {
                    int scrollY = _rowSizes.GetScroll(FirstVisibleRowScrollIndex, row);

                    _rowSizes.InvalidateAfterScroll(FirstVisibleRowScrollIndex, row, InvalidateRow, GridScrollAreaHeight);
                    FirstVisibleRowScrollIndex = row;

                    _drawBuffer.ScrollY(scrollY, GetScrollRect());
                    _drawBuffer.ScrollY(scrollY, GetRowHeadersScrollRect());
                    if (_columnSizes.FrozenCount > 0) _drawBuffer.ScrollY(scrollY, GetFrozenColumnsRect());
                }
                // if row heights are changed, invalidate all
                if (CountVisibleRowHeights())
                {
                    InvalidateAll();
                }
                if (IsTransposed) OnScrolledModelColumns();
                else OnScrolledModelRows();
                return;
            }

            if (column != FirstVisibleColumnScrollIndex && !_isInvalidatedAll && row == FirstVisibleRowScrollIndex
                && Math.Abs(column - FirstVisibleColumnScrollIndex) * 2 < VisibleColumnCount)
            {
                using (var ctx = CreateInvalidationContext())
                {
                    int scrollX = _columnSizes.GetScroll(FirstVisibleColumnScrollIndex, column);
                    _columnSizes.InvalidateAfterScroll(FirstVisibleColumnScrollIndex, column, InvalidateColumn, GridScrollAreaWidth);
                    FirstVisibleColumnScrollIndex = column;

                    _drawBuffer.ScrollX(scrollX, GetScrollRect());
                    _drawBuffer.ScrollX(scrollX, GetColumnHeadersScrollRect());
                    if (_rowSizes.FrozenCount > 0) _drawBuffer.ScrollX(scrollX, GetFrozenRowsRect());
                }
                if (IsTransposed) OnScrolledModelRows();
                else OnScrolledModelColumns();
                return;
            }

            bool changedRow = FirstVisibleRowScrollIndex != row;
            bool changedCol = FirstVisibleColumnScrollIndex != column;

            // render all
            using (var ctx = CreateInvalidationContext())
            {
                FirstVisibleRowScrollIndex = row;
                FirstVisibleColumnScrollIndex = column;
                CountVisibleRowHeights();
                InvalidateAll();
            }

            if (changedRow)
            {
                if (IsTransposed) OnScrolledModelColumns();
                else OnScrolledModelRows();
            }
            if (changedCol)
            {
                if (IsTransposed) OnScrolledModelRows();
                else OnScrolledModelColumns();
            }
        }
    }
}
