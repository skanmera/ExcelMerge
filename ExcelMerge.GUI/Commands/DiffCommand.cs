using System.Collections.Generic;
using System.IO;
using ExcelMerge.GUI.Views;
using ExcelMerge.GUI.ViewModels;

namespace ExcelMerge.GUI.Commands
{
    public class DiffCommand : ICommand
    {
        public static readonly List<string> DefaultEnabledExtensions = new List<string>
        {
            ".xls", ".xlsx", ".csv", "tsv",
        };

        public CommandLineOption Option { get; }

        public DiffCommand(CommandLineOption option)
        {
            Option = option;
        }

        public void Execute()
        {
            var window = new MainWindow();
            var diffView = new DiffView();
            var windowViewModel = new MainWindowViewModel(diffView);
            var diffViewModel = new DiffViewModel(Option.SrcPath, Option.DstPath, windowViewModel);
            window.DataContext = windowViewModel;
            diffView.DataContext = diffViewModel;

            App.Current.MainWindow = window;
            window.Show();
        }

        public void ValidateOption()
        {
            if (Option == null)
                throw new Exceptions.ExcelMergeException(true, "Option is null");

            if (!string.IsNullOrEmpty(Option.SrcPath) && Path.GetFileName(Option.SrcPath) == Option.EmptyFileName)
                Option.SrcPath = EnsureFile(Option.SrcPath);

            if (!string.IsNullOrEmpty(Option.DstPath) && Path.GetFileName(Option.DstPath) == Option.EmptyFileName)
                Option.DstPath = EnsureFile(Option.DstPath);

            if (Option.ValidateExtension)
            {
                if (!string.IsNullOrEmpty(Option.SrcPath) && !DefaultEnabledExtensions.Contains(Path.GetExtension(Option.SrcPath)) ||
                    !string.IsNullOrEmpty(Option.DstPath) && !DefaultEnabledExtensions.Contains(Path.GetExtension(Option.DstPath)))
                {
                    throw new Exceptions.ExcelMergeException(!Option.ImmediatelyExecuteExternalCommand, "Invalid extension.");
                }
            }
        }

        private string EnsureFile(string path)
        {
            if (!File.Exists(path))
                return CreateEmptyFile(ExcelWorkbookType.XLSX);

            var workbookType = ExcelUtility.GetWorkbookType(path);
            if (workbookType == ExcelWorkbookType.None)
                return CreateEmptyFile(ExcelWorkbookType.XLSX);

            return path;
        }

        private string CreateEmptyFile(ExcelWorkbookType workbookType)
        {
            var emptyFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".xlsx");

            ExcelUtility.CreateWorkbook(emptyFilePath, workbookType);

            return emptyFilePath;
        }
    }
}
