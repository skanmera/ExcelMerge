using System;
using System.ComponentModel;
using YamlDotNet.Serialization;

namespace ExcelMerge.GUI.Settings
{
    [Serializable]
    public class ExternalCommand : Setting<ExternalCommand>
    {
        private string name = string.Empty;
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        private string command = string.Empty;
        public string Command
        {
            get { return command; }
            set { SetProperty(ref command, value); }
        }

        private string args = string.Empty;
        public string Args
        {
            get { return args; }
            set { SetProperty(ref args, value); }
        }

        [YamlIgnore, IgnoreEqual]
        public bool CanExecute
        {
            get { return !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Convert(Command)); }
        }

        private bool isValid;
        [YamlIgnore, IgnoreEqual]
        public bool IsValid
        {
            get { return isValid; }
            private set { SetProperty(ref isValid, value); }
        }

        public ExternalCommand() { }

        public ExternalCommand(string name) : this()
        {
            Name = name;
        }

        public ExternalCommand(string name, string command, string args = "") : this(name)
        {
            Command = command;
            Args = args;
        }

        public override string ToString()
        {
            return Name;
        }

        protected override void OnPropertyChanged<TValue>(PropertyChangedEventArgs<TValue> args)
        {
            base.OnPropertyChanged(args);

            if (args.PropertyName == nameof(Name) || args.PropertyName == nameof(Command))
                IsValid = !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Command);
        }

        public void Execute(bool wait)
        {
            var process = System.Diagnostics.Process.Start(Convert(Command), Convert(Args));

            if (wait)
                process.WaitForExit();
        }

        private static string Convert(string str)
        {
            return str.Replace("${SRC}", EMEnvironmentValue.Get("SRC")).Replace("${DST}", EMEnvironmentValue.Get("DST"));
        }
    }
}
