using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastWpfGrid
{
    public interface IFastGridModel
    {
        int ColumnCount { get; }
        int RowCount { get; }
        IFastGridCell GetCell(IFastGridView grid, int row, int column);
        IFastGridCell GetRowHeader(IFastGridView view, int row);
        IFastGridCell GetColumnHeader(IFastGridView view, int column);
        IFastGridCell GetGridHeader(IFastGridView view);
        void AttachView(IFastGridView view);
        void DetachView(IFastGridView view);
        void HandleCommand(IFastGridView view, FastGridCellAddress address, object commandParameter, ref bool handled);

        HashSet<int> GetHiddenColumns(IFastGridView view);
        HashSet<int> GetFrozenColumns(IFastGridView view);
        HashSet<int> GetHiddenRows(IFastGridView view);
        HashSet<int> GetFrozenRows(IFastGridView view);

        void HandleSelectionCommand(IFastGridView view, string command);

        int? SelectedRowCountLimit { get; }
        int? SelectedColumnCountLimit { get; }
    }
}
