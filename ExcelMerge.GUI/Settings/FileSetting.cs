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

        private int columnHeaderIndex;
        public int ColumnHeaderIndex
        {
            get { return columnHeaderIndex; }
            set { SetProperty(ref columnHeaderIndex, value); Update(); }
        }

        private int rowHeaderIndex = -1;
        public int RowHeaderIndex
        {
            get { return rowHeaderIndex; }
            set { SetProperty(ref rowHeaderIndex, value); Update(); }
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

        private int maxRowHeaderWidth = 30;
        public int MaxRowHeaderWidth
        {
            get { return maxRowHeaderWidth; }
            set { SetProperty(ref maxRowHeaderWidth, value); Update(); }
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
                ColumnHeaderIndex.Equals(other.ColumnHeaderIndex) &&
                RowHeaderIndex.Equals(other.RowHeaderIndex) &&
                ExactMatch.Equals(other.ExactMatch) &&
                UseRegex.Equals(other.UseRegex) &&
                MaxRowHeaderWidth.Equals(other.MaxRowHeaderWidth);
        }

        public bool Ensure()
        {
            var changed = false;
            if (MaxRowHeaderWidth < 30)
            {
                MaxRowHeaderWidth = 30;
                changed |= true;
            }

            return changed;
        }

        public FileSetting Clone()
        {
            return new FileSetting()
            {
                Name = Name,
                ColumnHeaderIndex = ColumnHeaderIndex,
                RowHeaderIndex = RowHeaderIndex,
                ExactMatch = ExactMatch,
                UseRegex = UseRegex,
                MaxRowHeaderWidth = MaxRowHeaderWidth,
            };
        }

        private void Update()
        {
            IsValid = !string.IsNullOrWhiteSpace(Name);
        }
    }
}
