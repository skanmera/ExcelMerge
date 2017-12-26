using System;
using System.Linq;
using System.Reflection;
using YamlDotNet.Serialization;

namespace ExcelMerge.GUI.Settings
{
    [Serializable]
    public class FileSetting : Setting<FileSetting>
    {
        private string name = string.Empty;
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        private int sheetIndex;
        public int SheetIndex
        {
            get { return sheetIndex; }
            set { SetProperty(ref sheetIndex, value); }
        }

        private string sheetName = string.Empty;
        public string SheetName
        {
            get { return sheetName; }
            set { SetProperty(ref sheetName, value); }
        }

        private bool isStartupSheet;
        public bool IsStartupSheet
        {
            get { return isStartupSheet; }
            set { SetProperty(ref isStartupSheet, value); }
        }

        private int columnHeaderIndex;
        public int ColumnHeaderIndex
        {
            get { return columnHeaderIndex; }
            set { SetProperty(ref columnHeaderIndex, value); }
        }

        private int rowHeaderIndex = -1;
        public int RowHeaderIndex
        {
            get { return rowHeaderIndex; }
            set { SetProperty(ref rowHeaderIndex, value); }
        }

        private string rowHeaderName = string.Empty;
        public string RowHeaderName
        {
            get { return rowHeaderName; }
            set { SetProperty(ref rowHeaderName, value); }
        }

        private bool exactMatch;
        public bool ExactMatch
        {
            get { return exactMatch; }
            set { SetProperty(ref exactMatch, value); }
        }

        private bool useRegex;
        public bool UseRegex
        {
            get { return useRegex; }
            set { SetProperty(ref useRegex, value); }
        }

        private int maxRowHeaderWidth = 200;
        public int MaxRowHeaderWidth
        {
            get { return maxRowHeaderWidth; }
            set { SetProperty(ref maxRowHeaderWidth, value); }
        }

        private bool isValid;
        [YamlIgnore]
        public bool IsValid
        {
            get { return isValid; }
            private set { SetProperty(ref isValid, value); }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as FileSetting);
        }

        public override int GetHashCode()
        {
            var properties = GetType().GetProperties().Where(p => p.IsDefined(typeof(IgnoreEqualAttribute)));
            int hash = 17;

            unchecked
            {
                foreach (var property in properties)
                {
                    var value = property.GetValue(this);
                    if (value != null)
                        hash = hash * 23 + value.GetHashCode();
                }
            }

            return hash;
        }

        public override bool Ensure(bool isChanged = false)
        {
            if (MaxRowHeaderWidth < 200)
            {
                MaxRowHeaderWidth = 200;
                isChanged |= true;
            }

            return base.Ensure(isChanged);
        }

        protected override void OnPropertyChanged<TValue>(PropertyChangedEventArgs<TValue> args)
        {
            base.OnPropertyChanged(args);

            if (args.PropertyName == nameof(Name))
                IsValid = !string.IsNullOrWhiteSpace(Name);
        }
    }
}
