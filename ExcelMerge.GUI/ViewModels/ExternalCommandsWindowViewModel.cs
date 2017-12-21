using System.Linq;
using System.Windows;
using ExcelMerge.GUI.Settings;
using ExcelMerge.GUI.Views;

namespace ExcelMerge.GUI.ViewModels
{
    public class ExternalCommandsWindowViewModel : SettingCollectionWindowViewModelBase<ExternalCommand>
    {
        protected override SettingEditorWindowViewModelBase<ExternalCommand> CreateViewModel(ExternalCommand item)
        {
            return new ExternalCommandEditorWindowViewModel(item)
            {
                ValidateSettingCallback = Validate
            };
        }

        protected override Window CreateWindow(SettingEditorWindowViewModelBase<ExternalCommand> vm)
        {
            return new ExternalCommandEditorWindow
            {
                DataContext = vm
            };
        }

        private bool Validate(ExternalCommand externalCommand, ref string error)
        {
            if (SettingCollection.Any(ec => ec.Name == externalCommand.Name) && externalCommand.Name != selectedItem.Name)
            {
                error = $"{externalCommand.Name} is already exists.";
                return false;
            }

            return true;
        }

        protected override void Apply()
        {
            App.Instance.Setting.ExternalCommands = new ExternalCommandCollection(SettingCollection);

            base.Apply();
        }

        protected override void Reset()
        {
            SettingCollection = new ExternalCommandCollection(App.Instance.Setting.ExternalCommands);

            base.Reset();
        }
    }
}
