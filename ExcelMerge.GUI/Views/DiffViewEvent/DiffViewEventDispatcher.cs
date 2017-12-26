using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using FastWpfGrid;
using ExcelMerge.GUI.Settings;

namespace ExcelMerge.GUI.Views
{
    class DiffViewEventDispatcher<TSender, TListener>
    {
        public List<TListener> Listeners = new List<TListener>();

        public virtual void Dispatch(Action<TListener> action, DiffViewEventArgs<TSender> e)
        {
            if (e.TargetType == TargetType.All)
                Listeners.ForEach(l => action(l));
            else if (e.TargetType == TargetType.First && Listeners.Any())
                action(Listeners.First());
        }
    }

    class DataGridEventDispatcher : DiffViewEventDispatcher<FastGridControl, IDataGridEventListener>
    {
        private static DataGridEventDispatcher instance = new DataGridEventDispatcher();

        public static DataGridEventDispatcher Instance
        {
            get { return instance; }
        }

        public void DispatchParentLoadEvent(DiffViewEventArgs<FastGridControl> e)
        {
            Dispatch((l) => l.OnParentLoaded(e), e);
        }

        public void DispatchPreExecuteDiffEvent(DiffViewEventArgs<FastGridControl> e)
        {
            Dispatch((l) => l.OnPreExecuteDiff(e), e);
        }

        public void DispatchPostExecuteDiffEvent(DiffViewEventArgs<FastGridControl> e)
        {
            Dispatch((l) => l.OnPostExecuteDiff(e), e);
        }

        public void DispatchFileSettingUpdateEvent(DiffViewEventArgs<FastGridControl> e, FileSetting fileSetting)
        {
            Dispatch((l) => l.OnFileSettingUpdated(e, fileSetting), e);
        }

        public void DispatchApplicationSettingUpdateEvent(DiffViewEventArgs<FastGridControl> e)
        {
            Dispatch((l) => l.OnApplicationSettingUpdated(e), e);
        }

        public void DispatchScrollEvnet(DiffViewEventArgs<FastGridControl> e)
        {
            Dispatch((l) => l.OnScrolled(e), e);
        }

        public void DispatchSizeChangeEvent(DiffViewEventArgs<FastGridControl> e, SizeChangedEventArgs se)
        {
            Dispatch((l) => l.OnSizeChanged(e, se), e);
        }

        public void DispatchModelUpdateEvent(DiffViewEventArgs<FastGridControl> e)
        {
            Dispatch((l) => l.OnModelUpdated(e), e);
        }

        public void DispatchSelectedCellChangeEvent(DiffViewEventArgs<FastGridControl> e)
        {
            Dispatch((l) => l.OnSelectedCellChanged(e), e);
        }

        public void DispatchColumnHeaderChangeEvent(DiffViewEventArgs<FastGridControl> e)
        {
            Dispatch((l) => l.OnColumnHeaderChanged(e), e);
        }

        public void DispatchColumnHeaderResetEvent(DiffViewEventArgs<FastGridControl> e)
        {
            Dispatch((l) => l.OnColumnHeaderReset(e), e);
        }

        public void DispatchRowHeaderChagneEvent(DiffViewEventArgs<FastGridControl> e)
        {
            Dispatch((l) => l.OnRowHeaderChanged(e), e);
        }

        public void DispatchRowHeaderResetEvent(DiffViewEventArgs<FastGridControl> e)
        {
            Dispatch((l) => l.OnRowHeaderReset(e), e);
        }

        public void DispatchDisplayFormatChangeEvent(DiffViewEventArgs<FastGridControl> e, bool onlyDiff)
        {
            Dispatch((l) => l.OnDiffDisplayFormatChanged(e, onlyDiff), e);
        }

        public void DispatchColumnWidthChangeEvent(DiffViewEventArgs<FastGridControl> e, ColumnWidthChangedEventArgs ce)
        {
            Dispatch((l) => l.OnColumnWidthChanged(e, ce), e);
        }

        public void DispatchHoverRowChangeEvent(DiffViewEventArgs<FastGridControl> e, HoverRowChangedEventArgs he)
        {
            Dispatch((l) => l.OnHoverRowChanged(e, he), e);
        }
    }

    class LocationGridEventDispatcher : DiffViewEventDispatcher<Grid, ILocationGridEventListener>
    {
        private static LocationGridEventDispatcher instance = new LocationGridEventDispatcher();

        public static LocationGridEventDispatcher Instance
        {
            get { return instance; }
        }

        public void DispatchMouseDownEvent(DiffViewEventArgs<Grid> e, MouseEventArgs me)
        {
            Dispatch((l) => l.OnMouseDown(e, me), e);
        }

        public void DispatchMouseWheelEvent(DiffViewEventArgs<Grid> e, MouseWheelEventArgs me)
        {
            Dispatch((l) => l.OnMouseWheel(e, me), e);
        }

        public void DispatchSizeChangeEvent(DiffViewEventArgs<Grid> e, SizeChangedEventArgs se)
        {
            Dispatch((l) => l.OnSizeChanged(e, se), e);
        }
    }

    class ViewportEventDispatcher : DiffViewEventDispatcher<Rectangle, IViewportEventListener>
    {
        private static ViewportEventDispatcher instance = new ViewportEventDispatcher();

        public static ViewportEventDispatcher Instance
        {
            get { return instance; }
        }

        public void DispatchMoveEvent(DiffViewEventArgs<Rectangle> e)
        {
            Dispatch((l) => l.OnViewportMoved(e), e);
        }
    }

    class ValueTextBoxEventDispatcher : DiffViewEventDispatcher<RichTextBox, IValueTextBoxEventListener>
    {
        private static ValueTextBoxEventDispatcher instance = new ValueTextBoxEventDispatcher();

        public static ValueTextBoxEventDispatcher Instance
        {
            get { return instance; }
        }

        public void DispatchGotFocusEvent(DiffViewEventArgs<RichTextBox> e)
        {
            Dispatch((l) => l.OnGotFocus(e), e);
        }

        public void DispatchLostFocusEvent(DiffViewEventArgs<RichTextBox> e)
        {
            Dispatch((l) => l.OnLostFocus(e), e);
        }

        public void DispatchScrolledEvent(DiffViewEventArgs<RichTextBox> e, ScrollChangedEventArgs se)
        {
            Dispatch((l) => l.OnScrolled(e, se), e);
        }
    }
}
