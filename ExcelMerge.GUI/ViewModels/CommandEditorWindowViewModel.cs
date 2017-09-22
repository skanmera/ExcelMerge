using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Prism.Mvvm;
using Prism.Commands;
using ExcelMerge.GUI.Settings;

namespace ExcelMerge.GUI.ViewModels
{
    public class CommandEditorWindowViewModel : BindableBase
    {
        private string originalName;
        private List<ExternalCommand> commands;

        private ExternalCommand command;
        public ExternalCommand Command
        {
            get { return command; }
            private set { SetProperty(ref command, value); }
        }

        public bool IsDone { get; private set; }
        public DelegateCommand<Window> CancelCommand { get; private set; }
        public DelegateCommand<Window> ApplyCommand { get; private set; }

        public CommandEditorWindowViewModel(ExternalCommand command, List<ExternalCommand> commands)
        {
            this.commands = commands;
            this.originalName = command.Name;

            Command = new ExternalCommand(command.Name, command.Command, command.Args);

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
            if (commands.Any(c => c.Name == Command.Name && c.Name != originalName))
            {
                MessageBox.Show($"{Command.Name} is already exists.", "Failed", MessageBoxButton.OK);
                return;
            }

            IsDone = true;

            window.Close();
        }
    }
}
