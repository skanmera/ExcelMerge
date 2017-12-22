using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using FastWpfGrid;
using ExcelMerge.GUI.Settings;

namespace ExcelMerge.GUI.Views
{
    static class DataGridEventDispatcher
    {
        public static List<IDataGridEventHandler> Listeners = new List<IDataGridEventHandler>();

        public static void DispatchParentLoadEvent(IUnityContainer container)
        {
            Listeners.ForEach(l => l.OnParentLoaded(container));
        }

        public static void DispatchPreExecuteDiffEvent(IUnityContainer container)
        {
            Listeners.ForEach(l => l.OnPreExecuteDiff(container));
        }

        public static void DispatchPostExecuteDiffEvent(IUnityContainer container)
        {
            Listeners.ForEach(l => l.OnPostExecuteDiff(container));
        }

        public static void DispatchFileSettingUpdateEvent(FastGridControl target, IUnityContainer container, FileSetting fileSetting)
        {
            Listeners.ForEach(l => l.OnFileSettingUpdated(target, container, fileSetting));
        }

        public static void DispatchApplicationSettingUpdateEvent(IUnityContainer container)
        {
            Listeners.ForEach(l => l.OnApplicationSettingUpdated(container));
        }

        public static void DispatchScrollEvnet(FastGridControl target, IUnityContainer container)
        {
            Listeners.ForEach(l => l.OnScrolled(target, container));
        }

        public static void DispatchSizeChangeEvent(FastGridControl target, IUnityContainer contaier, SizeChangedEventArgs e)
        {
            Listeners.ForEach(l => l.OnSizeChanged(target, contaier, e));
        }

        public static void DispatchModelUpdateEvent(FastGridControl target, IUnityContainer container)
        {
            Listeners.ForEach(l => l.OnModelUpdated(target, container));
        }

        public static void DispatchSelectedCellChangeEvent(FastGridControl target, IUnityContainer container)
        {
            Listeners.ForEach(l => l.OnSelectedCellChanged(target, container));
        }

        public static void DispatchColumnHeaderChangeEvent(FastGridControl target, IUnityContainer container)
        {
            Listeners.ForEach(l => l.OnColumnHeaderChanged(target, container));
        }

        public static void DispatchColumnHeaderResetEvent(FastGridControl target, IUnityContainer container)
        {
            Listeners.ForEach(l => l.OnColumnHeaderReset(target, container));
        }

        public static void DispatchRowHeaderChagneEvent(FastGridControl target, IUnityContainer container)
        {
            Listeners.ForEach(l => l.OnRowHeaderChanged(target, container));
        }

        public static void DispatchRowHeaderResetEvent(FastGridControl target, IUnityContainer container)
        {
            Listeners.ForEach(l => l.OnRowHeaderReset(target, container));
        }

        public static void DispatchDisplayFormatChangeEvent(IUnityContainer container, bool onlyDiff)
        {
            Listeners.ForEach(l => l.OnDiffDisplayFormatChanged(container, onlyDiff));
        }

        public static void DispatchColumnWidthChangeEvent(FastGridControl target, IUnityContainer container, ColumnWidthChangedEventArgs e)
        {
            Listeners.ForEach(l => l.OnColumnWidthChanged(target, container, e));
        }
    }

    static class LocationGridEventDispatcher
    {
        public static List<ILocationGridEventHandler> Listeners = new List<ILocationGridEventHandler>();

        public static void DispatchMouseDownEvent(Grid target, IUnityContainer container, MouseEventArgs e)
        {
            Listeners.ForEach(l => l.OnMouseDown(target, container, e));
        }

        public static void DispatchMouseWheelEvent(Grid target, IUnityContainer container, MouseWheelEventArgs e)
        {
            Listeners.ForEach(l => l.OnMouseWheel(target, container, e));
        }

        public static void DispatchSizeChangeEvent(Grid target, IUnityContainer container, SizeChangedEventArgs e)
        {
            Listeners.ForEach(l => l.OnSizeChanged(target, container, e));
        }
    }

    static class ViewportEventDispatcher
    {
        public static List<IViewportEventListener> Listeners = new List<IViewportEventListener>();

        public static void DispatchMoveEvent(Rectangle target, IUnityContainer container)
        {
            Listeners.ForEach(l => l.OnViewportMoved(target, container));
        }
    }

    static class ValueTextBoxEventDispatcher
    {
        public static List<IValueTextBoxEventListener> Listeners = new List<IValueTextBoxEventListener>();

        public static void DispatchGotFocusEvent(RichTextBox target, IUnityContainer container)
        {
            Listeners.ForEach(l => l.OnGotFocus(target, container));
        }

        public static void DispatchLostFocusEvent(RichTextBox target, IUnityContainer container)
        {
            Listeners.ForEach(l => l.OnLostFocus(target, container));
        }

        public static void DispatchScrolledEvent(RichTextBox target, IUnityContainer container, ScrollChangedEventArgs e)
        {
            Listeners.ForEach(l => l.OnScrolled(target, container, e));
        }
    }
}
