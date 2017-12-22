using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using FastWpfGrid;
using ExcelMerge.GUI.Settings;

namespace ExcelMerge.GUI.Views
{
    interface IDataGridEventHandler
    {
        void OnParentLoaded(FastGridControl target, IUnityContainer container);
        void OnPreExecuteDiff(FastGridControl target, IUnityContainer container);
        void OnPostExecuteDiff(FastGridControl target, IUnityContainer container);
        void OnFileSettingUpdated(FastGridControl target, IUnityContainer container, FileSetting fileSetting);
        void OnApplicationSettingUpdated(FastGridControl target, IUnityContainer container);
        void OnScrolled(FastGridControl target, IUnityContainer container);
        void OnSizeChanged(FastGridControl target, IUnityContainer container, SizeChangedEventArgs e);
        void OnModelUpdated(FastGridControl target, IUnityContainer container);
        void OnSelectedCellChanged(FastGridControl target, IUnityContainer container);
        void OnColumnHeaderChanged(FastGridControl target, IUnityContainer container);
        void OnColumnHeaderReset(FastGridControl target, IUnityContainer container);
        void OnRowHeaderChanged(FastGridControl target, IUnityContainer container);
        void OnRowHeaderReset(FastGridControl target, IUnityContainer container);
        void OnDiffDisplayFormatChanged(FastGridControl target, IUnityContainer container, bool onlyDiff);
    }

    interface ILocationGridEventHandler
    {
        void OnMouseDown(Grid taget, IUnityContainer container, MouseEventArgs e);
        void OnMouseWheel(Grid target, IUnityContainer container, MouseWheelEventArgs e);
        void OnSizeChanged(Grid target, IUnityContainer container, SizeChangedEventArgs e);
    }

    interface IViewportEventListener
    {
        void OnViewportMoved(Rectangle target, IUnityContainer container);
    }

    interface IValueTextBoxEventListener
    {
        void OnGotFocus(RichTextBox target, IUnityContainer container);
        void OnLostFocus(RichTextBox target, IUnityContainer container);
        void OnScrolled(RichTextBox target, IUnityContainer container, ScrollChangedEventArgs e);
    }
}
