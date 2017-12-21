using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Prism.Mvvm;
using Prism.Commands;
using ExcelMerge.GUI.Settings;
using ExcelMerge.GUI.Views;

namespace ExcelMerge.GUI.ViewModels
{
    public class CommandsWindowViewModel : BindableBase
    {
        private List<ExternalCommand> externalCommands;
        public List<ExternalCommand> ExternalCommands
        {
            get { return externalCommands; }
            private set { SetProperty(ref externalCommands, value); }
        }

        private bool isDirty;
        public bool IsDirty
        {
            get { return isDirty; }
            private set { SetProperty(ref isDirty, value); }
        }

        public DelegateCommand<ExternalCommand> EditCommand { get; private set; }
        public DelegateCommand<ExternalCommand> RemoveCommand { get; private set; }
        public DelegateCommand ApplyCommand { get; private set; }
        public DelegateCommand ResetCommand { get; private set; }
        public DelegateCommand<Window> DoneCommand { get; private set; }

        public CommandsWindowViewModel()
        {
            ExternalCommands = App.Instance.Setting.ExternalCommands.ToList();

            EditCommand = new DelegateCommand<ExternalCommand>(OpenEditorWindow);
            RemoveCommand = new DelegateCommand<ExternalCommand>(RemoveExternalComand);
            ApplyCommand = new DelegateCommand(Apply);
            ResetCommand = new DelegateCommand(Reset);
            DoneCommand = new DelegateCommand<Window>(Done);
        }

        private void OpenEditorWindow(ExternalCommand command)
        {
            if (command == null)
                command = new ExternalCommand();

            var vm = new CommandEditorWindowViewModel(command, ExternalCommands);
            var window = new CommandEditorWindow()
            {
                DataContext = vm
            };

            window.ShowDialog();

            if (!vm.IsDone || command.Equals(vm.Command))
                return;

            var index = ExternalCommands.IndexOf(command);
            if (index >= 0)
            {
                RemoveExternalComand(command);
                ExternalCommands.Insert(index, vm.Command);
            }
            else
            {
                ExternalCommands.Add(vm.Command);
            }

            IsDirty = true;
            UpdateView();
        }

        private void RemoveExternalComand(ExternalCommand command)
        {
            IsDirty |= ExternalCommands.Remove(command);
            UpdateView();
        }

        private void UpdateView()
        {
            ExternalCommands = ExternalCommands.ToList();
        }

        private void Apply()
        {
            App.Instance.Setting.ExternalCommands = new ExternalCommandCollection(ExternalCommands);
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
            ExternalCommands = App.Instance.Setting.ExternalCommands.ToList();

            IsDirty = false;
        }
    }
}
