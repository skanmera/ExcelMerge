using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Windows.Media;
using YamlDotNet.Serialization;
using ExcelMerge.GUI.Styles;

namespace ExcelMerge.GUI.Settings
{
    [Serializable]
    public class ApplicationSetting : Setting<ApplicationSetting>
    {
        public static readonly string Location =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ExcelMerge", "ExcelMerge.yml");

        private bool skipFirstBlankRows;
        public bool SkipFirstBlankRows
        {
            get { return skipFirstBlankRows; }
            set { SetProperty(ref skipFirstBlankRows, value); }
        }

        private bool skipFirstBlankColumns;
        public bool SkipFirstBlankColumns
        {
            get { return skipFirstBlankColumns; }
            set { SetProperty(ref skipFirstBlankColumns, value); }
        }

        private bool trimLastBlankRows;
        public bool TrimLastBlankRows
        {
            get { return trimLastBlankRows; }
            set { SetProperty(ref trimLastBlankRows, value); }
        }

        private bool trimLastBlankColumns;
        public bool TrimLastBlankColumns
        {
            get { return trimLastBlankColumns; }
            set { SetProperty(ref trimLastBlankColumns, value); }
        }

        private ObservableCollection<string> alternatingColorStrings = new ObservableCollection<string>
        {
            "#FFFFFF", "#FAFAFA",
        };
        public ObservableCollection<string> AlternatingColorStrings
        {
            get { return alternatingColorStrings; }
            set { SetProperty(ref alternatingColorStrings, value); }
        }
        [YamlIgnore, IgnoreEqual]
        public Color[] AlternatingColors
        {
            get { return AlternatingColorStrings.Select(c => (Color)ColorConverter.ConvertFromString(c)).ToArray(); }
        }

        private string columnHeaderColorString;
        public string ColumnHeaderColorString
        {
            get { return columnHeaderColorString; }
            set { SetProperty(ref columnHeaderColorString, value); }
        }
        [YamlIgnore, IgnoreEqual]
        public Color ColumnHeaderColor
        {
            get { return (Color)ColorConverter.ConvertFromString(ColumnHeaderColorString); }
        }

        private string rowHeaderColorString;
        public string RowHeaderColorString
        {
            get { return rowHeaderColorString; }
            set { SetProperty(ref rowHeaderColorString, value); }
        }
        [YamlIgnore, IgnoreEqual]
        public Color RowHeaderColor
        {
            get { return (Color)ColorConverter.ConvertFromString(RowHeaderColorString); }
        }

        private string addedColorString;
        public string AddedColorString
        {
            get { return addedColorString; }
            set { SetProperty(ref addedColorString, value); }
        }
        [YamlIgnore, IgnoreEqual]
        public Color AddedColor
        {
            get { return (Color)ColorConverter.ConvertFromString(AddedColorString); }
        }

        private string removedColorString;
        public string RemovedColorString
        {
            get { return removedColorString; }
            set { SetProperty(ref removedColorString, value); }
        }
        [YamlIgnore, IgnoreEqual]
        public Color RemovedColor
        {
            get { return (Color)ColorConverter.ConvertFromString(RemovedColorString); }
        }

        private string modifiedColorString;
        public string ModifiedColorString
        {
            get { return modifiedColorString; }
            set { SetProperty(ref modifiedColorString, value); }
        }
        [YamlIgnore, IgnoreEqual]
        public Color ModifiedColor
        {
            get { return (Color)ColorConverter.ConvertFromString(modifiedColorString); }
        }

        private bool colorModifiedRow = true;
        public bool ColorModifiedRow
        {
            get { return colorModifiedRow; }
            set { SetProperty(ref colorModifiedRow, value); }
        }

        private string modifiedRowColorString;
        public string ModifiedRowColorString
        {
            get { return modifiedRowColorString; }
            set { SetProperty(ref modifiedRowColorString, value); }
        }
        [YamlIgnore, IgnoreEqual]
        public Color ModifiedRowColor
        {
            get { return (Color)ColorConverter.ConvertFromString(ModifiedRowColorString); }
        }

        private ObservableCollection<string> recentFileSets = new ObservableCollection<string>();
        public ObservableCollection<string> RecentFileSets
        {
            get { return recentFileSets; }
            set { SetProperty(ref recentFileSets, value); }
        }

        private ExternalCommandCollection externalCommands = new ExternalCommandCollection();
        public ExternalCommandCollection ExternalCommands
        {
            get { return externalCommands; }
            set { SetProperty(ref externalCommands, value); }
        }

        private FileSettingCollection fileSettings = new FileSettingCollection();
        public FileSettingCollection FileSettings
        {
            get { return fileSettings; }
            set { SetProperty(ref fileSettings, value); }
        }

        private string culture;
        public string Culture
        {
            get { return culture; }
            set { SetProperty(ref culture, value); }
        }

        private int cellWidth = 100;
        public int CellWidth
        {
            get { return cellWidth; }
            set { SetProperty(ref cellWidth, value); }
        }

        private ObservableCollection<string> searchHistory = new ObservableCollection<string>();
        public ObservableCollection<string> SearchHistory
        {
            get { return searchHistory; }
            set { SetProperty(ref searchHistory, value); }
        }

        private string fontName;
        public string FontName
        {
            get { return fontName; }
            set { SetProperty(ref fontName, value); }
        }

        private string logForamt = string.Empty;
        public string LogFormat
        {
            get { return logForamt; }
            set { SetProperty(ref logForamt, value); }
        }

        private string addedRowLogFormat = string.Empty;
        public string AddedRowLogFormat
        {
            get { return addedRowLogFormat; }
            set { SetProperty(ref addedRowLogFormat, value); }
        }

        private string removedRowLogFormat = string.Empty;
        public string RemovedRowLogFormat
        {
            get { return removedRowLogFormat; }
            set { SetProperty(ref removedRowLogFormat, value); }
        }

        public static ApplicationSetting Load()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Location));
            if (!File.Exists(Location))
                using (var fs = File.Create(Location)) { }

            ApplicationSetting setting = Deserialize(Location);
            if (setting == null)
            {
                setting = new ApplicationSetting();
                setting.Save();
            }

            return setting;
        }

        public void Save()
        {
            Serialize(this, Location);
        }

        public bool EnsureCulture(bool isChanged = false)
        {
            if (string.IsNullOrEmpty(Culture))
            {
                Culture = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
                isChanged |= true;
            }

            return isChanged;
        }

        public override bool Ensure(bool isChanged = false)
        {
            isChanged = EnsureCulture(isChanged);

            if (AlternatingColorStrings == null || !AlternatingColorStrings.Any())
            {
                AlternatingColorStrings = new ObservableCollection<string> { "#FFFFFF" };
                isChanged |= true;
            }

            if (string.IsNullOrEmpty(ColumnHeaderColorString))
            {
                ColumnHeaderColorString = EMColor.LightBlue.ToString();
                isChanged |= true;
            }

            if (string.IsNullOrEmpty(RowHeaderColorString))
            {
                RowHeaderColorString = EMColor.LightBlue.ToString();
                isChanged |= true;
            }

            if (string.IsNullOrEmpty(AddedColorString))
            {
                AddedColorString = EMColor.Orange.ToString();
                isChanged |= true;
            }

            if (string.IsNullOrEmpty(RemovedColorString))
            {
                RemovedColorString = EMColor.LightGray.ToString();
                isChanged |= true;
            }

            if (string.IsNullOrEmpty(ModifiedColorString))
            {
                ModifiedColorString = EMColor.Orange.ToString();
                isChanged |= true;
            }

            if (string.IsNullOrEmpty(ModifiedRowColorString))
            {
                ModifiedRowColorString = EMColor.PaleOrange.ToString();
                isChanged |= true;
            }

            foreach (var ec in ExternalCommands)
            {
                isChanged |= ec.Ensure();
            }

            foreach (var fs in FileSettings)
            {
                isChanged |= fs.Ensure();
            }

            if (string.IsNullOrEmpty(FontName))
            {
                FontName = "Arial";
                isChanged |= true;
            }

            if (string.IsNullOrEmpty(LogFormat))
            {
                LogFormat = Properties.Resources.DefaultLogFormat;
                isChanged |= true;
            }

            if (string.IsNullOrEmpty(AddedRowLogFormat))
            {
                AddedRowLogFormat = Properties.Resources.DefaultLogFormatAddedRow;
                isChanged |= true;
            }

            if (string.IsNullOrEmpty(RemovedRowLogFormat))
            {
                RemovedRowLogFormat = Properties.Resources.DefaultLogFormatRemovedRow;
                isChanged |= true;
            }

            return isChanged;
        }

        private static void Serialize(ApplicationSetting setting, string path)
        {
            var serializer = new SerializerBuilder().EmitDefaults().Build();
            var yml = serializer.Serialize(setting);
            using (var sr = new StreamWriter(path))
            {
                sr.Write(yml);
            }
        }

        private static ApplicationSetting Deserialize(string path)
        {
            using (var sr = new StreamReader(path))
            {
                using (var input = new StringReader(sr.ReadToEnd()))
                {
                    var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
                    return deserializer.Deserialize<ApplicationSetting>(input);
                }
            }
        }
    }
}
