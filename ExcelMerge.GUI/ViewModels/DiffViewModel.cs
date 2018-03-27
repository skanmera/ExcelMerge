using ExcelMerge.GUI.Behaviors;
using Prism.Mvvm;
using SKCore.Collection;
using SKCore.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace ExcelMerge.GUI.ViewModels
{
    internal class DiffViewModel : BindableBase
    {
        private bool showLocationGridLine;
        public bool ShowLocationGridLine
        {
            get { return showLocationGridLine; }
            set { SetProperty(ref showLocationGridLine, value); }
        }

        private string srcPath;
        public string SrcPath
        {
            get { return srcPath; }
            set
            {
                SetProperty(ref srcPath, value);
                Settings.EMEnvironmentValue.Set("SRC", value);
                UpdateExecutableFlag();
            }
        }

        private string dstPath;
        public string DstPath
        {
            get { return dstPath; }
            set
            {
                SetProperty(ref dstPath, value);
                Settings.EMEnvironmentValue.Set("DST", value);
                UpdateExecutableFlag();
            }
        }

        private List<string> originalSrcSheetNames = new List<string>();

        private List<string> srcSheetNames;
        public List<string> SrcSheetNames
        {
            get { return srcSheetNames; }
            private set { SetProperty(ref srcSheetNames, value); }
        }

        private List<string> originalDstSheetNames = new List<string>();

        private List<string> dstSheetNames;
        public List<string> DstSheetNames
        {
            get { return dstSheetNames; }
            private set { SetProperty(ref dstSheetNames, value); }
        }

        private int selectedSrcSheetIndex;
        public int SelectedSrcSheetIndex
        {
            get { return selectedSrcSheetIndex; }
            set
            {
                SetProperty(ref selectedSrcSheetIndex, value);
                UpdateOtherSheetsExecutableFlag();
            }
        }

        private int selectedDstSheetIndex;
        public int SelectedDstSheetIndex
        {
            get { return selectedDstSheetIndex; }
            set
            {
                SetProperty(ref selectedDstSheetIndex, value);
                UpdateOtherSheetsExecutableFlag();
            }
        }

        private bool executable;
        public bool Executable
        {
            get { return executable; }
            private set
            {
                SetProperty(ref executable, value);
                UpdateOtherSheetsExecutableFlag();
            }
        }

        private bool executableNext;
        public bool ExecutableNext
        {
            get { return executableNext; }
            private set { SetProperty(ref executableNext, value); }
        }

        private bool executablePrev;
        public bool ExecutablePrev
        {
            get { return executablePrev; }
            private set { SetProperty(ref executablePrev, value); }
        }

        private int modifiedCellCount;
        public int ModifiedCellCount
        {
            get { return modifiedCellCount; }
            private set { SetProperty(ref modifiedCellCount, value); }
        }

        private int modifiedRowCount;
        public int ModifiedRowCount
        {
            get { return modifiedRowCount; }
            private set { SetProperty(ref modifiedRowCount, value); }
        }

        private int addedRowCount;
        public int AddedRowCount
        {
            get { return addedRowCount; }
            private set { SetProperty(ref addedRowCount, value); }
        }

        private int removedRowCount;
        public int RemovedRowCount
        {
            get { return removedRowCount; }
            private set { SetProperty(ref removedRowCount, value); }
        }

        private DragAcceptDescription description;
        public DragAcceptDescription Description
        {
            get { return description; }
            private set { SetProperty(ref description, value); }
        }

        private ObservableCollection<ExcelSheetDiffInfo> sheetDiffInfoList 
            = new ObservableCollection<ExcelSheetDiffInfo>();
        public ObservableCollection<ExcelSheetDiffInfo> SheetDiffInfoList
        {
            get { return sheetDiffInfoList; }
            private set { SetProperty(ref sheetDiffInfoList, value); }
        }

        public DiffViewModel()
        {
            Description = new DragAcceptDescription();
            Description.DragDrop += DragDrop;
            Description.DragDrop += DragOver;

            SrcPath = string.Empty;
            DstPath = string.Empty;
        }

        public DiffViewModel(string src, string dst, MainWindowViewModel mwv) : this()
        {
            SrcPath = src;
            DstPath = dst;

            mwv.PropertyChanged += Mwv_PropertyChanged;
        }

        public void UpdateDiffSummary(ExcelSheetDiffSummary summary)
        {
            ModifiedCellCount = summary.ModifiedCellCount;
            ModifiedRowCount = summary.ModifiedRowCount;
            AddedRowCount = summary.AddedRowCount;
            RemovedRowCount = summary.RemovedRowCount;
        }

        private void Mwv_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SrcPath))
            {
                var vm = sender as MainWindowViewModel;
                if (vm != null)
                {
                    var prop = typeof(MainWindowViewModel).GetProperties().FirstOrDefault(p => p.Name == e.PropertyName);
                    if (prop != null)
                    {
                        SrcPath = prop.GetValue(vm) as string;
                    }
                }
            }
            else if (e.PropertyName == nameof(DstPath))
            {
                var vm = sender as MainWindowViewModel;
                if (vm != null)
                {
                    var prop = typeof(MainWindowViewModel).GetProperties().FirstOrDefault(p => p.Name == e.PropertyName);
                    if (prop != null)
                    {
                        DstPath = prop.GetValue(vm) as string;
                    }
                }
            }
        }

        private void DragDrop(DragEventArgs e)
        {
            var paths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (paths == null || !paths.Any())
                return;

            var target = e.Source as FrameworkElement;
            if (target == null)
                return;

            OnDragDrop(paths, target);
        }

        protected virtual void OnDragDrop(string[] filePath, FrameworkElement target)
        {
            if (filePath.Length > 1)
            {
                SrcPath = filePath[1];
                DstPath = filePath[0];

                return;
            }

            var tag = Convert.ToInt32(target.Tag);
            if (tag == 0)
                SrcPath = filePath[0];
            else if (tag == 1)
                DstPath = filePath[0];
        }

        private void DragOver(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;

            e.Handled = true;
        }

        private void UpdateExecutableFlag()
        {
            var existsSrc = File.Exists(SrcPath);
            var existsDst = File.Exists(DstPath);

            if (existsSrc)
            {
                var tmp = Path.ChangeExtension(App.GetTempFileName(), Path.GetExtension(SrcPath));
                PathUtility.CopyTree(SrcPath, tmp, overwrite: true);
                File.SetAttributes(tmp, FileAttributes.Normal);
                originalSrcSheetNames = ExcelWorkbook.GetSheetNames(tmp).ToList();
                SelectedSrcSheetIndex = 0;
            }
            else
            {
                originalSrcSheetNames = new List<string>();
                SelectedSrcSheetIndex = -1;
            }

            if (existsDst)
            {
                var tmp = Path.ChangeExtension(App.GetTempFileName(), Path.GetExtension(DstPath));
                PathUtility.CopyTree(DstPath, tmp, overwrite: true);
                File.SetAttributes(tmp, FileAttributes.Normal);
                originalDstSheetNames = ExcelWorkbook.GetSheetNames(tmp).ToList();
                SelectedDstSheetIndex = 0;
            }
            else
            {
                originalDstSheetNames = new List<string>();
                SelectedDstSheetIndex = -1;
            }

            SrcSheetNames = new List<string>();
            DstSheetNames = new List<string>();

            if (App.Instance.Setting.MakeSheetPairsByName)
            {
                var intersection = originalSrcSheetNames.Intersect(originalDstSheetNames);
                SrcSheetNames = intersection.ToList();
                DstSheetNames = intersection.ToList();

                foreach (var srcSheetName in originalSrcSheetNames.Except(intersection))
                {
                    SrcSheetNames.Add(srcSheetName);
                    DstSheetNames.Add(Path.GetRandomFileName());
                }

                foreach (var dstSheetName in originalDstSheetNames.Except(intersection))
                {
                    SrcSheetNames.Add(Path.GetRandomFileName());
                    DstSheetNames.Add(dstSheetName);
                }
            }
            else
            {
                for (int i = 0, max = Math.Max(originalSrcSheetNames.Count, originalDstSheetNames.Count); i < max; i++)
                {
                    SrcSheetNames.Add(originalSrcSheetNames.ElementAtOrElse(i, Path.GetRandomFileName()));
                    DstSheetNames.Add(originalDstSheetNames.ElementAtOrElse(i, Path.GetRandomFileName()));
                }
            }

            Executable = existsSrc && existsDst && (SrcSheetNames.Any() && DstSheetNames.Any());
        }

        private void UpdateOtherSheetsExecutableFlag()
        {
            ExecutableNext = Executable &&
                SelectedSrcSheetIndex + 1 < SrcSheetNames.Count &&
            SelectedDstSheetIndex + 1 < dstSheetNames.Count;

            ExecutablePrev = Executable &&
                SelectedSrcSheetIndex - 1 >= 0 &&
                SelectedDstSheetIndex - 1 >= 0;
        }

        public void AddSheetDiff(ExcelSheetDiff diff)
        {
            if (SheetDiffInfoList
                .Any(i => i.SheetDiff.SrcSheet.Name == diff.SrcSheet.Name && i.SheetDiff.DstSheet.Name == diff.DstSheet.Name))
                return;

            SheetDiffInfoList.Add(new ExcelSheetDiffInfo(diff));
        }

        public int GetSrcSheetIndex(string name)
        {
            return SrcSheetNames.IndexOf(name);
        }

        public int GetDstSheetIndex(string name)
        {
            return DstSheetNames.IndexOf(name);
        }

        public int GetSrcSheetIndex(int origIndex)
        {
            var name = originalSrcSheetNames.ElementAtOrDefault(origIndex);

            return GetSrcSheetIndex(name);
        }

        public int GetDstSheetIndex(int origIndex)
        {
            var name = originalDstSheetNames.ElementAtOrDefault(origIndex);

            return GetDstSheetIndex(name);
        }

        public IEnumerable<Tuple<string, string>> GetSheetNamePairs()
        {
            return SrcSheetNames.Zip(DstSheetNames, (s, d) => Tuple.Create(s, d));
        }
    }

    internal class ExcelSheetDiffInfo
    {
        public ExcelSheetDiff SheetDiff { get; }
        public bool IsNotified { get; set; }

        public ExcelSheetDiffInfo(ExcelSheetDiff sheetDiff)
        {
            SheetDiff = sheetDiff;
        }

        public override string ToString()
        {
            return SheetDiff?.ToString();
        }
    }
}
