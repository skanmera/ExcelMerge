using System.Windows;
using Prism.Mvvm;
using Prism.Commands;
using ExcelMerge.GUI.Settings;

namespace ExcelMerge.GUI.ViewModels
{
    public abstract class SettingCollectionWindowViewModelBase<T> : BindableBase where T : Setting<T>, new()
    {
        protected T selectedItem;

        private SettingCollection<T> settingCollection;
        public SettingCollection<T> SettingCollection
        {
            get { return settingCollection; }
            protected set { SetProperty(ref settingCollection, value); }
        }

        private bool isDirty;
        public bool IsDirty
        {
            get { return isDirty; }
            private set { SetProperty(ref isDirty, value); }
        }

        public DelegateCommand<T> EditCommand { get; private set; }
        public DelegateCommand<T> RemoveCommand { get; private set; }
        public DelegateCommand ApplyCommand { get; private set; }
        public DelegateCommand ResetCommand { get; private set; }
        public DelegateCommand<Window> DoneCommand { get; private set; }

        public SettingCollectionWindowViewModelBase()
        {
            Reset();

            EditCommand = new DelegateCommand<T>(Edit);
            RemoveCommand = new DelegateCommand<T>(Remove);
            ApplyCommand = new DelegateCommand(Apply);
            ResetCommand = new DelegateCommand(Reset);
            DoneCommand = new DelegateCommand<Window>(Done);
        }

        protected void Edit(T item)
        {
            if (item == null)
                item = new T();

            selectedItem = item;

            var vm = CreateViewModel(item);
            var window = CreateWindow(vm);
            window.ShowDialog();

            if (vm.IsCancelled || item.Equals(vm.Setting))
                return;

            var index = SettingCollection.IndexOf(item);
            if (index >= 0)
            {
                Remove(item);
                SettingCollection.Insert(index, vm.Setting);
            }
            else
            {
                SettingCollection.Add(vm.Setting);
            }

            IsDirty = true;
        }

        protected virtual void Apply()
        {
            App.Instance.Setting.Save();
            Reset();
        }

        protected void Done(Window window)
        {
            Apply();
            window.Close();
        }

        protected virtual void Reset()
        {
            IsDirty = false;
        }

        protected virtual void Remove(T item)
        {
            IsDirty |= SettingCollection.Remove(item);
        }

        protected abstract SettingEditorWindowViewModelBase<T> CreateViewModel(T item);
        protected abstract Window CreateWindow(SettingEditorWindowViewModelBase<T> vm);
    }
}
