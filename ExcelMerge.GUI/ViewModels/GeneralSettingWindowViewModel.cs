using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Prism.Mvvm;
using Prism.Commands;
using ExcelMerge.GUI.Settings;

namespace ExcelMerge.GUI.ViewModels
{
    public class GeneralSettingWindowViewModel : BindableBase
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

        public DelegateCommand<Window> DoneCommand { get; private set; }
        public DelegateCommand ResetCommand { get; private set; }
        public DelegateCommand ApplyCommand { get; private set; }

        public GeneralSettingWindowViewModel()
        {
            originalSetting = App.Instance.Setting;
            Setting = originalSetting.Clone();

            Setting.PropertyChanged += Setting_PropertyChanged;

            DoneCommand = new DelegateCommand<Window>(Done);
            ResetCommand = new DelegateCommand(Reset);
            ApplyCommand = new DelegateCommand(Apply);
        }

        private void Setting_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            IsDirty = !Setting.Equals(originalSetting);
        }

        private void Done(Window window)
        {
            Apply();

            window.Close();
        }

        private void Reset()
        {
            Setting = originalSetting.Clone();

            IsDirty = false;
        }

        private void Apply()
        {
            App.Instance.UpdateSetting(Setting);
            App.Instance.Setting.Save();

            originalSetting = App.Instance.Setting;

            IsDirty = false;
        }
    }
}
