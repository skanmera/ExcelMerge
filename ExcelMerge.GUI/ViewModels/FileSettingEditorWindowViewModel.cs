using ExcelMerge.GUI.Settings;

namespace ExcelMerge.GUI.ViewModels
{
    public class FileSettingEditorWindowViewModel : SettingEditorWindowViewModelBase<FileSetting>
    {
        public FileSettingEditorWindowViewModel(FileSetting fileSetting)
            : base(fileSetting)
        { }
    }
}
