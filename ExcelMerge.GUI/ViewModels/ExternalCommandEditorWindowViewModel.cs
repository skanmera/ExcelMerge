using ExcelMerge.GUI.Settings;

namespace ExcelMerge.GUI.ViewModels
{
    public class ExternalCommandEditorWindowViewModel : SettingEditorWindowViewModelBase<ExternalCommand>
    {
        public ExternalCommandEditorWindowViewModel(ExternalCommand externalCommand)
            : base(externalCommand)
        { }
    }
}
