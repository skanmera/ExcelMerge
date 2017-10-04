using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Windows.Media;
using Prism.Mvvm;
using YamlDotNet.Serialization;

namespace ExcelMerge.GUI.Settings
{
    public class ApplicationSetting : BindableBase, ICloneable<ApplicationSetting>, IEquatable<ApplicationSetting>
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
                    var deserializer = new DeserializerBuilder().Build();
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

        private void Serialize()
        {
            var serializer = new Serializer();
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
            clone.ExternalCommands = ExternalCommands.Select(c => c.Clone()).ToList();
            clone.RecentFileSets = RecentFileSets.ToList();
            clone.CellWidth = CellWidth;
            clone.AlternatingColorStrings = new ObservableCollection<string>(AlternatingColorStrings);

            return clone;
        }

        public bool Equals(ApplicationSetting other)
        {
            return
                SkipFirstBlankRows == other.skipFirstBlankRows &&
                ExternalCommands.SequenceEqual(other.ExternalCommands) &&
                RecentFileSets.SequenceEqual(other.RecentFileSets) &&
                CellWidth == other.cellWidth &&
                AlternatingColorStrings.SequenceEqual(other.AlternatingColorStrings);
        }
    }
}
