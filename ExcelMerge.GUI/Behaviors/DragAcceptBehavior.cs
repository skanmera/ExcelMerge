using System.Windows;
using System.Windows.Interactivity;

namespace ExcelMerge.GUI.Behaviors
{
    public sealed class DragAcceptBehavior : Behavior<FrameworkElement>
    {
        public DragAcceptDescription Description
        {
            get { return (DragAcceptDescription)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(DragAcceptDescription),
            typeof(DragAcceptBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            this.AssociatedObject.PreviewDragOver += OnDragOverAssociatedObject;
            this.AssociatedObject.PreviewDrop += OnDropAssociatedObject;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewDragOver -= OnDragOverAssociatedObject;
            AssociatedObject.PreviewDrop -= OnDropAssociatedObject;
            base.OnDetaching();
        }

        private void OnDragOverAssociatedObject(object sender, DragEventArgs e)
        {
            var desc = Description;
            if (desc == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            desc.OnDragOver(e);
            e.Handled = true;
        }

        private void OnDropAssociatedObject(object sender, DragEventArgs e)
        {
            var desc = Description;
            if (desc == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            desc.OnDrop(e);
            e.Handled = true;
        }
    }
}
