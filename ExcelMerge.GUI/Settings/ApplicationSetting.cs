using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Windows.Media;
using Prism.Mvvm;
using YamlDotNet.Serialization;
using ExcelMerge.GUI.Styles;

namespace ExcelMerge.GUI.Settings
{
    public class ApplicationSetting : BindableBase, IEquatable<ApplicationSetting>
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
        [YamlIgnore]
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
        [YamlIgnore]
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
        [YamlIgnore]
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
        [YamlIgnore]
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
        [YamlIgnore]
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
        [YamlIgnore]
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
        [YamlIgnore]
        public Color ModifiedRowColor
        {
            get { return (Color)ColorConverter.ConvertFromString(ModifiedRowColorString); }
        }

        private List<string> recentFileSets = new List<string>();
        public List<string> RecentFileSets
        {
            get { return recentFileSets; }
            set { SetProperty(ref recentFileSets, value); }
        }

        private List<ExternalCommand> externalCommands = new List<ExternalCommand>();
        public List<ExternalCommand> ExternalCommands
        {
            get { return externalCommands; }
            set { SetProperty(ref externalCommands, value); }
        }

        private List<FileSetting> fileSettings = new List<FileSetting>();
        public List<FileSetting> FileSettings
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

        private string cellBaseLogFormat = string.Empty;
        public string CellBaseLogFormat
        {
            get { return cellBaseLogFormat; }
            set { SetProperty(ref cellBaseLogFormat, value); }
        }

        private string rowBaseLogFormat = string.Empty;
        public string RowBaseLogFormat
        {
            get { return rowBaseLogFormat; }
            set { SetProperty(ref rowBaseLogFormat, value); }
        }

        private string columnBaseLogFormat = string.Empty;
        public string ColumnBaseLogFormat
        {
            get { return columnBaseLogFormat; }
            set { SetProperty(ref columnBaseLogFormat, value); }
        }

        public static ApplicationSetting Load()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Location));
            if (!File.Exists(Location))
                using (var fs = File.Create(Location)) { }

            ApplicationSetting setting = null;
            using (var sr = new StreamReader(Location))
            {
                using (var input = new StringReader(sr.ReadToEnd()))
                {
                    var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
                    setting = deserializer.Deserialize<ApplicationSetting>(input);
                }
            }

            if (setting == null)
            {
                setting = new ApplicationSetting();
                setting.Save();
            }

            return setting;
        }

        public void Save()
        {
            Serialize();
        }

        public bool Ensure()
        {
            bool changed = false;
            if (string.IsNullOrEmpty(Culture))
            {
                Culture = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
                changed |= true;
            }

            if (AlternatingColorStrings == null || !AlternatingColorStrings.Any())
            {
                AlternatingColorStrings = new ObservableCollection<string> { "#FFFFFF" };
                changed |= true;
            }

            if (string.IsNullOrEmpty(ColumnHeaderColorString))
            {
                ColumnHeaderColorString = EMColor.LightBlue.ToString();
                changed |= true;
            }

            if (string.IsNullOrEmpty(RowHeaderColorString))
            {
                RowHeaderColorString = EMColor.LightBlue.ToString();
                changed |= true;
            }

            if (string.IsNullOrEmpty(AddedColorString))
            {
                AddedColorString = EMColor.Orange.ToString();
                changed |= true;
            }

            if (string.IsNullOrEmpty(RemovedColorString))
            {
                RemovedColorString = EMColor.LightGray.ToString();
                changed |= true;
            }

            if (string.IsNullOrEmpty(ModifiedColorString))
            {
                ModifiedColorString = EMColor.Orange.ToString();
                changed |= true;
            }

            if (string.IsNullOrEmpty(ModifiedRowColorString))
            {
                ModifiedRowColorString = EMColor.PaleOrange.ToString();
                changed |= true;
            }

            foreach (var ec in ExternalCommands)
            {
                changed |= ec.Ensure();
            }

            foreach (var fs in FileSettings)
            {
                changed |= fs.Ensure();
            }

            if (string.IsNullOrEmpty(FontName))
            {
                FontName = "Arial";
                changed |= true;
            }

            if (string.IsNullOrEmpty(CellBaseLogFormat))
            {
                CellBaseLogFormat = Properties.Resources.DefaultCellBaseLogFormat;
                changed |= true;
            }

            if (string.IsNullOrEmpty(RowBaseLogFormat))
            {
                RowBaseLogFormat = Properties.Resources.DefaultRowBaseLogFormat;
                changed |= true;
            }

            if (string.IsNullOrEmpty(CellBaseLogFormat))
            {
                CellBaseLogFormat = Properties.Resources.DefaultColumnBaseLogFormat;
                changed |= true;
            }

            return changed;
        }

        private void Serialize()
        {
            var serializer = new SerializerBuilder().EmitDefaults().Build();
            var yml = serializer.Serialize(this);
            using (var sr = new StreamWriter(Location))
            {
                sr.Write(yml);
            }
        }

        public ApplicationSetting Clone()
        {
            var clone = new ApplicationSetting();
            clone.SkipFirstBlankRows = SkipFirstBlankRows;
            clone.SkipFirstBlankColumns = SkipFirstBlankColumns;
            clone.TrimLastBlankRows = TrimLastBlankRows;
            clone.TrimLastBlankColumns = TrimLastBlankColumns;
            clone.ExternalCommands = ExternalCommands.Select(c => c.Clone()).ToList();
            clone.FileSettings = FileSettings.Select(f => f.Clone()).ToList();
            clone.RecentFileSets = RecentFileSets.ToList();
            clone.CellWidth = CellWidth;
            clone.AlternatingColorStrings = new ObservableCollection<string>(AlternatingColorStrings);
            clone.ColumnHeaderColorString = ColumnHeaderColorString;
            clone.RowHeaderColorString = RowHeaderColorString;
            clone.AddedColorString = AddedColorString;
            clone.RemovedColorString = RemovedColorString;
            clone.modifiedColorString = ModifiedColorString;
            clone.ModifiedRowColorString = ModifiedRowColorString;
            clone.ColorModifiedRow = ColorModifiedRow;
            clone.SearchHistory = new ObservableCollection<string>(SearchHistory);
            clone.FontName = FontName;
            clone.CellBaseLogFormat = CellBaseLogFormat;
            clone.RowBaseLogFormat = RowBaseLogFormat;
            clone.ColumnBaseLogFormat = ColumnBaseLogFormat;

            return clone;
        }

        public bool Equals(ApplicationSetting other)
        {
            return
                SkipFirstBlankRows == other.skipFirstBlankRows &&
                SkipFirstBlankColumns == other.SkipFirstBlankColumns &&
                TrimLastBlankRows == other.TrimLastBlankRows &&
                TrimLastBlankColumns == other.TrimLastBlankColumns &&
                ExternalCommands.SequenceEqual(other.ExternalCommands) &&
                FileSettings.SequenceEqual(other.FileSettings) &&
                RecentFileSets.SequenceEqual(other.RecentFileSets) &&
                CellWidth == other.cellWidth &&
                AlternatingColorStrings.SequenceEqual(other.AlternatingColorStrings) &&
                ColumnHeaderColorString.Equals(other.ColumnHeaderColorString) &&
                RowHeaderColorString.Equals(other.RowHeaderColorString) &&
                AddedColorString.Equals(other.AddedColorString) &&
                RemovedColorString.Equals(other.RemovedColorString) &&
                ModifiedColorString.Equals(other.ModifiedColorString) &&
                ModifiedRowColorString.Equals(other.ModifiedRowColorString) &&
                ColorModifiedRow.Equals(other.ColorModifiedRow) &&
                SearchHistory.Equals(other.SearchHistory) &&
                FontName.Equals(other.FontName) &&
                CellBaseLogFormat.Equals(other.CellBaseLogFormat) &&
                RowBaseLogFormat.Equals(other.RowBaseLogFormat) &&
                ColumnBaseLogFormat.Equals(other.ColumnBaseLogFormat);

        }
    }
}
