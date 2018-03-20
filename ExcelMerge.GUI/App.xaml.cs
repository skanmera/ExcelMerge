using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using ExcelMerge.GUI.Commands;
using ExcelMerge.GUI.Settings;

namespace ExcelMerge.GUI
{
    public partial class App : Application
    {
        public static readonly string TempDirectory = Path.Combine(
            Path.GetTempPath(),
            ((GuidAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(GuidAttribute))).Value,
            DateTime.Now.Ticks.ToString());

        public ApplicationSetting Setting { get; private set; }
        public CommandLineOption CommandLineOption { get; private set; }

        public event Action OnSettingUpdated;

        [STAThread()]
        public static void Main()
        {
            App app = new App();
            app.InitializeComponent();
            app.Setting = ApplicationSetting.Load();
            app.Setting.EnsureCulture();
            app.UpdateResourceCulture();

            if (app.Setting.Ensure())
                app.Setting.Save();

            app.Run();
        }

        public static App Instance
        {
            get { return (App)Current; }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            base.OnStartup(e);

            var args = Environment.GetCommandLineArgs().Skip(1).ToList();
            if (!args.Any())
                args.Add(CommandType.Diff.ToString());

            CommandLineOption = new CommandLineOption();

            var command = CreateCommand(args.ToArray());
            command.ValidateOption();
            command.Execute();
        }

        private void StoreOption()
        {
            EMEnvironmentValue.Set("SRC", CommandLineOption.SrcPath);
            EMEnvironmentValue.Set("DST", CommandLineOption.DstPath);
        }

        private ICommand CreateCommand(string[] args)
        {
            if (CommandLine.Parser.Default.ParseArguments(args, CommandLineOption))
            {
                StoreOption();
                CommandLineOption.ConvertToFullPath();
                return CommandFactory.Create(CommandLineOption);
            }

            throw new Exceptions.ExcelMergeException(true, $"Invalid argument.\nargument:\n{string.Join(" ", args)}");
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            if (exception != null)
            {
                bool showDialog = true;
                bool executeExternalCommand = true;
                if (exception is Exceptions.ExcelMergeException)
                    showDialog = (exception as Exceptions.ExcelMergeException).ShowDialog;

                if (showDialog)
                {
                    var message = $"Execute external command ? \n\n------------------------------------\n {exception.Message}\n{exception.StackTrace}";
                    var result = MessageBox.Show(message, "An error occurred.", MessageBoxButton.YesNo);
                    executeExternalCommand = result == MessageBoxResult.Yes;
                }

                if (executeExternalCommand)
                    ExecuteExternalCommand();
            }

            Environment.Exit(-1);
        }

        public void ExecuteExternalCommand()
        {
            var command = Setting.ExternalCommands.FirstOrDefault(c => c.Name == CommandLineOption.ExternalCommand);
            if (command == null)
                return;

            command.Execute(CommandLineOption.WaitExternalCommand);
        }

        public void UpdateSetting(ApplicationSetting setting)
        {
            Setting = setting.DeepClone();

            if (OnSettingUpdated == null)
                OnSettingUpdated += () => { };

            OnSettingUpdated();
        }

        public void UpdateRecentFiles(string srcPath, string dstPath)
        {
            var updated = Setting.RecentFileSets.ToList();
            var key = srcPath + "|" + dstPath;
            var index = updated.IndexOf(key);
            if (index >= 0)
            {
                updated.RemoveAt(index);
                updated.Insert(0, key);
            }
            else
            {
                updated.Insert(0, srcPath + "|" + dstPath);
            }

            while (updated.Count > 20)
            {
                updated.RemoveAt(updated.Count - 1);
            }

            Setting.RecentFileSets = new System.Collections.ObjectModel.ObservableCollection<string>(updated);
            Setting.Save();
        }

        public void UpdateResourceCulture()
        {
            if (string.IsNullOrEmpty(Setting.Culture))
                return;

            if (GUI.Properties.Resources.Culture != null)
            {
                if (GUI.Properties.Resources.Culture.Name == Setting.Culture)
                    return;

                MessageBox.Show(GUI.Properties.Resources.Message_Reboot);
            }

            GUI.Properties.Resources.Culture = new System.Globalization.CultureInfo(Setting.Culture);
        }

        public IEnumerable<string> GetRecentFiles()
        {
            return Setting.RecentFileSets.SelectMany(f => f.Split('|'));
        }

        public IEnumerable<string> GetRecentSrcFiles()
        {
            return Setting.RecentFileSets.Select(f => f.Split('|').ElementAtOrDefault(0));
        }

        public IEnumerable<string> GetRecentDstFiles()
        {
            return Setting.RecentFileSets.Select(f => f.Split('|').ElementAtOrDefault(1));
        }

        public IEnumerable<Tuple<string, string>> GetRecentFileSets()
        {
            return Setting.RecentFileSets.Select(f =>
            {
                var files = f.Split('|');
                return Tuple.Create(files.ElementAtOrDefault(0), files.ElementAtOrDefault(1));
            });
        }

        public bool KeepFileHistory
        {
            get { return CommandLineOption.KeepFileHistory; }
        }

        public static string GetTempFileName()
        {
            return Path.Combine(TempDirectory, Path.GetRandomFileName());
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                if (Directory.Exists(TempDirectory))
                    Directory.Delete(TempDirectory, true);
            }
            catch
            {
            }
        }
    }
}
