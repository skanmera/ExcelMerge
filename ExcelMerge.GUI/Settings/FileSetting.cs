using System;
using Prism.Mvvm;

namespace ExcelMerge.GUI.Settings
{
    public class FileSetting : BindableBase, IEquatable<FileSetting>
    {
        private string name = string.Empty;
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); Update(); }
        }

        private int headerIndex;
        public int HeaderIndex
        {
            get { return headerIndex; }
            set { SetProperty(ref headerIndex, value); Update(); }
        }

        private int frozenColumnCount;
        public int FrozenColumnCount
        {
            get { return frozenColumnCount; }
            set { SetProperty(ref frozenColumnCount, value); Update(); }
        }

        private bool exactMatch;
        public bool ExactMatch
        {
            get { return exactMatch; }
            set { SetProperty(ref exactMatch, value); Update(); }
        }

        private bool useRegex;
        public bool UseRegex
        {
            get { return useRegex; }
            set { SetProperty(ref useRegex, value); Update(); }
        }

        private bool isValid;
        [YamlDotNet.Serialization.YamlIgnore]
        public bool IsValid
        {
            get { return isValid; }
            private set { SetProperty(ref isValid, value); }
        }

        public bool Equals(FileSetting other)
        {
            return
                Name.Equals(other.name) &&
                HeaderIndex.Equals(other.HeaderIndex) &&
                FrozenColumnCount.Equals(other.FrozenColumnCount) &&
                ExactMatch.Equals(other.ExactMatch) &&
                UseRegex.Equals(other.UseRegex);
        }

        public FileSetting Clone()
        {
            return new FileSetting()
            {
                Name = Name,
                HeaderIndex = HeaderIndex,
                FrozenColumnCount = FrozenColumnCount,
                ExactMatch = ExactMatch,
                UseRegex = UseRegex,
            };
        }

        private void Update()
        {
            IsValid = !string.IsNullOrWhiteSpace(Name);
        }
    }
}
