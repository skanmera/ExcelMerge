using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Prism.Mvvm;
using Prism.Commands;
using Xceed.Wpf.Toolkit;
using ExcelMerge.GUI.Settings;

namespace ExcelMerge.GUI.ViewModels
{
    public class DiffExtractionSettingWindowViewModel : BindableBase
    {
        private ApplicationSetting originalSetting;

        private ApplicationSetting setting;
        public ApplicationSetting Setting
        {
            get { return setting; }
            private set { SetProperty(ref setting, value); }
        }

        private bool isDirty;
        public bool IsDirty
        {
            get { return isDirty; }
            private set { SetProperty(ref isDirty, value); }
        }

        private bool canRemoveAlternationColor;
        public bool CanRemoveAlternationColor
        {
            get { return canRemoveAlternationColor; }
            private set { SetProperty(ref canRemoveAlternationColor, value); }
        }

        public DelegateCommand<Window> DoneCommand { get; private set; }
        public DelegateCommand ResetCommand { get; private set; }
        public DelegateCommand ApplyCommand { get; private set; }
        public DelegateCommand<object> EditAlternationColorCommand { get; private set; }
        public DelegateCommand<int?> RemoveAlternationColorCommand { get; private set; }
        public DelegateCommand<Color?> AddAlternationColorCommand { get; private set; }

        public DiffExtractionSettingWindowViewModel()
        {
            originalSetting = App.Instance.Setting;
            Setting = originalSetting.Clone();

            CanRemoveAlternationColor = Setting.AlternatingColorStrings.Count > 1;

            Setting.PropertyChanged += Setting_PropertyChanged;

            DoneCommand = new DelegateCommand<Window>(Done);
            ResetCommand = new DelegateCommand(Reset);
            ApplyCommand = new DelegateCommand(Apply);

            EditAlternationColorCommand = new DelegateCommand<object>(EditAlternationColor);
            RemoveAlternationColorCommand = new DelegateCommand<int?>(RemoveAlternationColor);
            AddAlternationColorCommand = new DelegateCommand<Color?>(AddAlternationColor);
        }

        private void Setting_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateDirtyFlag();
        }

        private void UpdateDirtyFlag()
        {
            IsDirty = !Setting.Equals(originalSetting);
        }

        private void Done(Window window)
        {
            if (IsDirty)
                Apply();

            window.Close();
        }

        private void Reset()
        {
            Setting.PropertyChanged -= Setting_PropertyChanged;
            Setting = originalSetting.Clone();
            Setting.PropertyChanged += Setting_PropertyChanged;

            IsDirty = false;
        }

        private void Apply()
        {
            App.Instance.UpdateSetting(Setting);
            App.Instance.Setting.Save();

            originalSetting = App.Instance.Setting;

            IsDirty = false;
        }

        private void EditAlternationColor(object parameter)
        {
            var parameters = parameter as List<object>;

            if (parameters?.Count < 2)
                return;

            var index = Convert.ToInt32(parameters[0]);
            var color = parameters[1].ToString();

            Setting.AlternatingColorStrings[index] = color;

            UpdateDirtyFlag();
        }

        private void RemoveAlternationColor(int? index)
        {
            if (!index.HasValue)
                return;

            Setting.AlternatingColorStrings.RemoveAt(index.Value);
            CanRemoveAlternationColor = Setting.AlternatingColorStrings.Count > 1;

            UpdateDirtyFlag();
        }

        private void AddAlternationColor(Color? color)
        {
            if (!color.HasValue)
                return;

            Setting.AlternatingColorStrings.Add(color.ToString());
            CanRemoveAlternationColor = Setting.AlternatingColorStrings.Count > 1;

            UpdateDirtyFlag();
        }
    }
}
