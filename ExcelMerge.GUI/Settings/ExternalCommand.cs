using System;
using Prism.Mvvm;

namespace ExcelMerge.GUI.Settings
{
    public class ExternalCommand : BindableBase, IEquatable<ExternalCommand>
    {
        private string name = string.Empty;
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); Update(); }
        }

        private string command = string.Empty;
        public string Command
        {
            get { return command; }
            set { SetProperty(ref command, value); Update(); }
        }

        private string args = string.Empty;
        public string Args
        {
            get { return args; }
            set { SetProperty(ref args, value); Update(); }
        }

        [YamlDotNet.Serialization.YamlIgnore]
        public bool CanExecute
        {
            get { return !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Convert(Command)); }
        }

        private bool isValid;
        [YamlDotNet.Serialization.YamlIgnore]
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

        public override bool Equals(object obj)
        {
            var other = obj as ExternalCommand;
            if (other == null)
                return false;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            var hash = 17;
            hash = hash * 23 + Name.GetHashCode();
            hash = hash * 23 + Command.GetHashCode();
            hash = hash * 23 + Args.GetHashCode();

            return hash;
        }

        public bool Equals(ExternalCommand other)
        {
            if (other == null)
                return false;

            return Name.Equals(other.Name) && Command.Equals(other.Command) && Args.Equals(other.Args);
        }

        private void Update()
        {
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

        public ExternalCommand Clone()
        {
            return new ExternalCommand(Name, Command, Args);
        }

        public bool Ensure()
        {
            return false;
        }
    }
}
