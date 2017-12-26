using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using FastWpfGrid;
using ExcelMerge.GUI.Settings;

namespace ExcelMerge.GUI.Views
{
    interface IDataGridEventListener
    {
        void OnParentLoaded(DiffViewEventArgs<FastGridControl> e);
        void OnPreExecuteDiff(DiffViewEventArgs<FastGridControl> e);
        void OnPostExecuteDiff(DiffViewEventArgs<FastGridControl> e);
        void OnFileSettingUpdated(DiffViewEventArgs<FastGridControl> e, FileSetting fileSetting);
        void OnApplicationSettingUpdated(DiffViewEventArgs<FastGridControl> e);
        void OnScrolled(DiffViewEventArgs<FastGridControl> e);
        void OnSizeChanged(DiffViewEventArgs<FastGridControl> e, SizeChangedEventArgs se);
        void OnModelUpdated(DiffViewEventArgs<FastGridControl> e);
        void OnSelectedCellChanged(DiffViewEventArgs<FastGridControl> e);
        void OnColumnHeaderChanged(DiffViewEventArgs<FastGridControl> e);
        void OnColumnHeaderReset(DiffViewEventArgs<FastGridControl> e);
        void OnRowHeaderChanged(DiffViewEventArgs<FastGridControl> e);
        void OnRowHeaderReset(DiffViewEventArgs<FastGridControl> e);
        void OnDiffDisplayFormatChanged(DiffViewEventArgs<FastGridControl> e, bool onlyDiff);
        void OnColumnWidthChanged(DiffViewEventArgs<FastGridControl> e, ColumnWidthChangedEventArgs ce);
        void OnHoverRowChanged(DiffViewEventArgs<FastGridControl> e, HoverRowChangedEventArgs he);
    }

    interface ILocationGridEventListener
    {
        void OnMouseDown(DiffViewEventArgs<Grid> e, MouseEventArgs me);
        void OnMouseWheel(DiffViewEventArgs<Grid> e, MouseWheelEventArgs me);
        void OnSizeChanged(DiffViewEventArgs<Grid> e, SizeChangedEventArgs me);
    }

    interface IViewportEventListener
    {
        void OnViewportMoved(DiffViewEventArgs<Rectangle> e);
    }

    interface IValueTextBoxEventListener
    {
        void OnGotFocus(DiffViewEventArgs<RichTextBox> e);
        void OnLostFocus(DiffViewEventArgs<RichTextBox> e);
        void OnScrolled(DiffViewEventArgs<RichTextBox> e, ScrollChangedEventArgs se);
    }
}
