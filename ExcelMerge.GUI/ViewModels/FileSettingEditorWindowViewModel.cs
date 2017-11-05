using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Prism.Mvvm;
using Prism.Commands;
using ExcelMerge.GUI.Settings;

namespace ExcelMerge.GUI.ViewModels
{
    public class FileSettingEditorWindowViewModel : BindableBase
    {
        private string originalName;
        private List<FileSetting> fileSettings;

        private FileSetting fileSetting;
        public FileSetting FileSetting
        {
            get { return fileSetting; }
            set { SetProperty(ref fileSetting, value); }
        }

        public bool IsDone { get; private set; }
        public DelegateCommand<Window> CancelCommand { get; private set; }
        public DelegateCommand<Window> ApplyCommand { get; private set; }

        public FileSettingEditorWindowViewModel(FileSetting fileSetting, List<FileSetting> fileSettings)
        {
            this.fileSettings = fileSettings;
            this.originalName = fileSetting.Name;

            FileSetting = fileSetting.Clone();

            CancelCommand = new DelegateCommand<Window>(Cancel);
            ApplyCommand = new DelegateCommand<Window>(Apply);
        }

        private void Cancel(Window window)
        {
            if (window != null)
                window.Close();
        }

        private void Apply(Window window)
        {
            //if (fileSettings.Any(f => f.Name == FileSetting.Name && f.Name != originalName))
            //{
            //    MessageBox.Show($"{FileSetting.Name} is already exists.", "Failed", MessageBoxButton.OK);
            //    return;
            //}

            IsDone = true;

            window.Close();
        }
    }
}
