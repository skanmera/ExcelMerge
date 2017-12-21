using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Prism.Mvvm;
using Prism.Commands;
using ExcelMerge.GUI.Settings;
using ExcelMerge.GUI.Views;

namespace ExcelMerge.GUI.ViewModels
{
    public class FileSettingsWindowViewModel : BindableBase
    {
        private List<FileSetting> fileSettings;
        public List<FileSetting> FileSettings
        {
            get { return fileSettings; }
            private set { SetProperty(ref fileSettings, value); }
        }

        private bool isDirty;
        public bool IsDirty
        {
            get { return isDirty; }
            private set { SetProperty(ref isDirty, value); }
        }

        public DelegateCommand<FileSetting> EditCommand { get; private set; }
        public DelegateCommand<FileSetting> RemoveCommand { get; private set; }
        public DelegateCommand ApplyCommand { get; private set; }
        public DelegateCommand ResetCommand { get; private set; }
        public DelegateCommand<Window> DoneCommand { get; private set; }

        public FileSettingsWindowViewModel()
        {
            Refresh();

            EditCommand = new DelegateCommand<FileSetting>(OpenEditorWindow);
            RemoveCommand = new DelegateCommand<FileSetting>(RemoveFileSetting);
            ApplyCommand = new DelegateCommand(Apply);
            ResetCommand = new DelegateCommand(Reset);
            DoneCommand = new DelegateCommand<Window>(Done);
        }

        private void OpenEditorWindow(FileSetting fileSetting)
        {
            if (fileSetting == null)
                fileSetting = new FileSetting();

            var vm = new FileSettingEditorWindowViewModel(fileSetting, FileSettings);
            var window = new FileSettingEditorWindow()
            {
                DataContext = vm
            };

            window.ShowDialog();

            if (!vm.IsDone || fileSetting.Equals(vm.FileSetting))
                return;

            var index = FileSettings.IndexOf(fileSetting);
            if (index >= 0)
            {
                RemoveFileSetting(fileSetting);
                FileSettings.Insert(index, vm.FileSetting);
            }
            else
            {
                FileSettings.Add(vm.FileSetting);
            }

            IsDirty = true;
            UpdateView();
        }

        private void RemoveFileSetting(FileSetting fileSetting)
        {
            IsDirty |= FileSettings.Remove(fileSetting);
            UpdateView();
        }

        private void UpdateView()
        {
            FileSettings = FileSettings.ToList();
        }

        private void Apply()
        {
            App.Instance.Setting.FileSettings = new FileSettingCollection(FileSettings);
            App.Instance.Setting.Save();

            Refresh();
        }

        private void Reset()
        {
            Refresh();
        }

        private void Done(Window window)
        {
            Apply();
            window.Close();
        }

        private void Refresh()
        {
            FileSettings = App.Instance.Setting.FileSettings.ToList();

            IsDirty = false;
        }
    }
}
