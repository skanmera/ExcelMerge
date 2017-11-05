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

        private int sheetIndex;
        public int SheetIndex
        {
            get { return sheetIndex; }
            set { SetProperty(ref sheetIndex, value); Update(); }
        }

        private string sheetName = string.Empty;
        public string SheetName
        {
            get { return sheetName; }
            set { SetProperty(ref sheetName, value); Update(); }
        }

        private bool isStartupSheet;
        public bool IsStartupSheet
        {
            get { return isStartupSheet; }
            set { SetProperty(ref isStartupSheet, value); Update(); }
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

        private string rowHeaderName = string.Empty;
        public string RowHeaderName
        {
            get { return rowHeaderName; }
            set { SetProperty(ref rowHeaderName, value); Update(); }
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

        private int maxRowHeaderWidth = 200;
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
                SheetIndex.Equals(other.SheetIndex) &&
                SheetName.Equals(other.SheetName) &&
                IsStartupSheet.Equals(other.IsStartupSheet) &&
                ColumnHeaderIndex.Equals(other.ColumnHeaderIndex) &&
                RowHeaderIndex.Equals(other.RowHeaderIndex) &&
                RowHeaderName.Equals(other.RowHeaderName) &&
                ExactMatch.Equals(other.ExactMatch) &&
                UseRegex.Equals(other.UseRegex) &&
                MaxRowHeaderWidth.Equals(other.MaxRowHeaderWidth);
        }

        public bool Ensure()
        {
            var changed = false;
            if (MaxRowHeaderWidth < 200)
            {
                MaxRowHeaderWidth = 200;
                changed |= true;
            }

            return changed;
        }

        public FileSetting Clone()
        {
            return new FileSetting()
            {
                Name = Name,
                SheetIndex = SheetIndex,
                SheetName = SheetName,
                IsStartupSheet = IsStartupSheet,
                ColumnHeaderIndex = ColumnHeaderIndex,
                RowHeaderIndex = RowHeaderIndex,
                RowHeaderName = RowHeaderName,
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
