using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FastWpfGrid
{
    partial class FastGridControl
    {
        private bool _isInvalidated;
        private bool _isInvalidatedAll;
        private bool _InvalidatedGridHeader;
        private List<int> _invalidatedRows = new List<int>();
        private List<int> _invalidatedColumns = new List<int>();
        private List<Tuple<int, int>> _invalidatedCells = new List<Tuple<int, int>>();
        private List<int> _invalidatedRowHeaders = new List<int>();
        private List<int> _invalidatedColumnHeaders = new List<int>();

        private class InvalidationContext : IDisposable
        {
            private FastGridControl _grid;

            internal InvalidationContext(FastGridControl grid)
            {
                _grid = grid;
                _grid.EnterInvalidation();
            }

            public void Dispose()
            {
                _grid.LeaveInvalidation();
            }
        }

        private int _invalidationCount;

        private void LeaveInvalidation()
        {
            _invalidationCount--;
            if (_invalidationCount == 0)
            {
                if (_isInvalidated)
                {
                    RenderGrid();
                }
            }
        }

        private void EnterInvalidation()
        {
            _invalidationCount++;
        }

        private InvalidationContext CreateInvalidationContext()
        {
            return new InvalidationContext(this);
        }

        private void CheckInvalidation()
        {
            if (_isInvalidated) return;
            if (_invalidationCount > 0) return;
            Dispatcher.BeginInvoke(DispatcherPriority.Render, (Action) RenderInvoked);
        }

        private void RenderInvoked()
        {
            if (!_isInvalidated) return;
            RenderGrid();
        }

        public void InvalidateAll()
        {
            CheckInvalidation();
            _isInvalidatedAll = true;
            _isInvalidated = true;
        }

        public void InvalidateRowHeader(int row)
        {
            CheckInvalidation();
            _isInvalidated = true;
            _invalidatedRowHeaders.Add(row);
        }

        public void InvalidateColumnHeader(int column)
        {
            CheckInvalidation();
            _isInvalidated = true;
            _invalidatedColumnHeaders.Add(column);
        }

        public void InvalidateColumn(int column)
        {
            CheckInvalidation();
            _isInvalidated = true;
            _invalidatedColumns.Add(column);
            _invalidatedColumnHeaders.Add(column);
        }

        public void InvalidateRow(int row)
        {
            CheckInvalidation();
            _isInvalidated = true;
            _invalidatedRows.Add(row);
            _invalidatedRowHeaders.Add(row);
        }

        public void InvalidateCell(int row, int column)
        {
            CheckInvalidation();
            _isInvalidated = true;
            _invalidatedCells.Add(Tuple.Create(row, column));
        }

        public void InvalidateGridHeader()
        {
            CheckInvalidation();
            _isInvalidated = true;
            _InvalidatedGridHeader = true;
        }

        public void InvalidateCell(FastGridCellAddress cell)
        {
            if (cell.IsEmpty) return;
            if (cell.IsGridHeader)
            {
                InvalidateGridHeader();
                return;
            }
            if (cell.IsRowHeader)
            {
                InvalidateRowHeader(cell.Row.Value);
                return;
            }
            if (cell.IsColumnHeader)
            {
                InvalidateColumnHeader(cell.Column.Value);
                return;
            }
            InvalidateCell(cell.Row.Value, cell.Column.Value);
        }

        private void ClearInvalidation()
        {
            _invalidatedRows.Clear();
            _invalidatedColumns.Clear();
            _invalidatedCells.Clear();
            _invalidatedColumnHeaders.Clear();
            _invalidatedRowHeaders.Clear();
            _isInvalidated = false;
            _isInvalidatedAll = false;
            _InvalidatedGridHeader = false;
        }

        private bool ShouldDrawCell(int row, int column)
        {
            if (!_isInvalidated || _isInvalidatedAll) return true;

            if (_invalidatedRows.Contains(row)) return true;
            if (_invalidatedColumns.Contains(column)) return true;
            if (_invalidatedCells.Contains(Tuple.Create(row, column))) return true;
            return false;
        }

        private bool ShouldDrawRowHeader(int row)
        {
            if (!_isInvalidated || _isInvalidatedAll) return true;

            if (_invalidatedRows.Contains(row)) return true;
            if (_invalidatedRowHeaders.Contains(row)) return true;
            return false;
        }

        private bool ShouldDrawColumnHeader(int column)
        {
            if (!_isInvalidated || _isInvalidatedAll) return true;

            if (_invalidatedColumns.Contains(column)) return true;
            if (_invalidatedColumnHeaders.Contains(column)) return true;
            return false;
        }

        private bool ShouldDrawGridHeader()
        {
            if (!_isInvalidated || _isInvalidatedAll) return true;
            return _InvalidatedGridHeader;
        }

        public void InvalidateModelCell(int row, int column)
        {
            InvalidateCell(ModelToReal(new FastGridCellAddress(row, column)));
        }

        public void InvalidateModelRowHeader(int row)
        {
            InvalidateCell(ModelToReal(new FastGridCellAddress(row, null)));
        }
        public void InvalidateModelRow(int row)
        {
            if (IsTransposed)
            {
                InvalidateColumn(_columnSizes.ModelToReal(row));
            }
            else
            {
                InvalidateRow(_rowSizes.ModelToReal(row));
            }
        }
        public void InvalidateModelColumnHeader(int column)
        {
            InvalidateCell(ModelToReal(new FastGridCellAddress(null, column)));
        }
        public void InvalidateModelColumn(int column)
        {
            if (IsTransposed)
            {
                InvalidateRow(_rowSizes.ModelToReal(column));
            }
            else
            {
                InvalidateColumn(_columnSizes.ModelToReal(column));
            }
        }
    }
}
