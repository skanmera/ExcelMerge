using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Practices.Unity;
using FastWpfGrid;
using NetDiff;
using SKCore.IO;
using SKCore.Collection;
using ExcelMerge.GUI.ViewModels;
using ExcelMerge.GUI.Settings;
using ExcelMerge.GUI.Models;
using ExcelMerge.GUI.Styles;

namespace ExcelMerge.GUI.Views
{
    public partial class DiffView : UserControl
    {
        private ExcelSheetDiffConfig diffConfig = new ExcelSheetDiffConfig();
        private IUnityContainer container;
        private const string srcKey = "src";
        private const string dstKey = "dst";
        private bool isLock = false;

        private FastGridControl copyTargetGrid;

        public DiffView()
        {
            InitializeComponent();
            InitializeContainer();
            InitializeEventListeners();

            App.Instance.OnSettingUpdated += OnApplicationSettingUpdated;

            SearchTextCombobox.ItemsSource = App.Instance.Setting.SearchHistory.ToList();

            // In order to enable Ctrl + F immediately after startup.
            ToolExpander.IsExpanded = true;

            SrcDataGrid.ScrolledBeyondEnd += OnScrolledBeyondEnd;
            DstDataGrid.ScrolledBeyondEnd += OnScrolledBeyondEnd;
            SrcDataGrid.ScrolledBeyondStart += OnScrolledBeyondStart;
            DstDataGrid.ScrolledBeyondStart += OnScrolledBeyondStart;
        }

        private DiffViewModel GetViewModel()
        {
            return DataContext as DiffViewModel;
        }

        private void InitializeContainer()
        {
            container = new UnityContainer();
            container
                .RegisterInstance(srcKey, SrcDataGrid)
                .RegisterInstance(dstKey, DstDataGrid)
                .RegisterInstance(srcKey, SrcLocationGrid)
                .RegisterInstance(dstKey, DstLocationGrid)
                .RegisterInstance(srcKey, SrcViewRectangle)
                .RegisterInstance(dstKey, DstViewRectangle)
                .RegisterInstance(srcKey, SrcValueTextBox)
                .RegisterInstance(dstKey, DstValueTextBox);
        }

        private void InitializeEventListeners()
        {
            var srcEventHandler = new DiffViewEventHandler(srcKey);
            var dstEventHandler = new DiffViewEventHandler(dstKey);

            DataGridEventDispatcher.Instance.Listeners.Add(srcEventHandler);
            DataGridEventDispatcher.Instance.Listeners.Add(dstEventHandler);
            LocationGridEventDispatcher.Instance.Listeners.Add(srcEventHandler);
            LocationGridEventDispatcher.Instance.Listeners.Add(dstEventHandler);
            ViewportEventDispatcher.Instance.Listeners.Add(srcEventHandler);
            ViewportEventDispatcher.Instance.Listeners.Add(dstEventHandler);
            ValueTextBoxEventDispatcher.Instance.Listeners.Add(srcEventHandler);
            ValueTextBoxEventDispatcher.Instance.Listeners.Add(dstEventHandler);
        }

        private void OnApplicationSettingUpdated()
        {
            var e = new DiffViewEventArgs<FastGridControl>(null, container, TargetType.First);
            DataGridEventDispatcher.Instance.DispatchApplicationSettingUpdateEvent(e);
        }

        public void OnScrolledBeyondEnd(object sender, EventArgs e)
        {
            if (App.Instance.Setting.MoveNextSheetWhenScrolledBeyondEnd)
                ShowNextSheetDiff();
        }

        public void OnScrolledBeyondStart(object sender, EventArgs e)
        {
            if (App.Instance.Setting.MovePrevSheetWhenScrolledBeyondStart)
                ShowPrevSheetDiff();
        }

        private void ShowNextSheetDiff()
        {
            if (isLock)
                return;

            if (SrcSheetCombobox.SelectedIndex + 1 >= SrcSheetCombobox.Items.Count ||
                DstSheetCombobox.SelectedIndex + 1 >= DstSheetCombobox.Items.Count)
                return;

            isLock = true;

            SrcSheetCombobox.SelectedIndex = SrcSheetCombobox.SelectedIndex + 1;
            diffConfig.SrcSheetIndex = SrcSheetCombobox.SelectedIndex;
            DstSheetCombobox.SelectedIndex = DstSheetCombobox.SelectedIndex + 1;
            diffConfig.DstSheetIndex = DstSheetCombobox.SelectedIndex;

            ExecuteDiff(useCache: true);

            isLock = false;
        }

        private void ShowPrevSheetDiff()
        {
            if (isLock)
                return;

            if (SrcSheetCombobox.SelectedIndex - 1 < 0 ||
                DstSheetCombobox.SelectedIndex - 1 < 0)
                return;

            isLock = true;

            SrcSheetCombobox.SelectedIndex = SrcSheetCombobox.SelectedIndex - 1;
            diffConfig.SrcSheetIndex = SrcSheetCombobox.SelectedIndex;
            DstSheetCombobox.SelectedIndex = DstSheetCombobox.SelectedIndex - 1;
            diffConfig.DstSheetIndex = DstSheetCombobox.SelectedIndex;

            ExecuteDiff(useCache: true);

            isLock = false;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var args = new DiffViewEventArgs<FastGridControl>(null, container, TargetType.First);
            DataGridEventDispatcher.Instance.DispatchParentLoadEvent(args);

            ExecuteDiff(isStartup: true);

            // In order to enable Ctrl + F immediately after startup.
            ToolExpander.IsExpanded = false;
        }

        private ExcelSheetDiffConfig CreateDiffConfig(FileSetting fileSetting, bool isStartup)
        {
            var config = new ExcelSheetDiffConfig();

            config.SrcSheetIndex = SrcSheetCombobox.SelectedIndex;
            config.DstSheetIndex = DstSheetCombobox.SelectedIndex;

            if (fileSetting != null)
            {
                if (isStartup)
                {
                    config.SrcSheetIndex = Math.Max(GetSheetIndex(fileSetting, SrcSheetCombobox.Items), 0);
                    config.DstSheetIndex = Math.Max(GetSheetIndex(fileSetting, DstSheetCombobox.Items), 0);
                }

                config.SrcHeaderIndex = fileSetting.ColumnHeaderIndex;
                config.DstHeaderIndex = fileSetting.ColumnHeaderIndex;
            }

            return config;
        }

        private int GetSheetIndex(FileSetting fileSetting, ItemCollection sheetNames, bool dst = false)
        {
            if (fileSetting == null)
                return -1;

            var index = fileSetting.SheetIndex;
            if (!string.IsNullOrEmpty(fileSetting.SheetName))
                index = sheetNames.IndexOf(fileSetting.SheetName);

            if (index < 0 || index >= sheetNames.Count)
            {
                MessageBox.Show(Properties.Resources.Msg_OutofSheetRange);
                index = 0;
            }

            return dst ? GetViewModel().GetDstSheetIndex(index) : GetViewModel().GetSrcSheetIndex(index);
        }

        private void LocationGrid_MouseDown(object sender, MouseEventArgs e)
        {
            var args = new DiffViewEventArgs<Grid>(sender as Grid, container);
            LocationGridEventDispatcher.Instance.DispatchMouseDownEvent(args, e);
        }

        private void LocationGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var args = new DiffViewEventArgs<Grid>(sender as Grid, container);
                LocationGridEventDispatcher.Instance.DispatchMouseDownEvent(args, e);
            }
        }

        private void LocationGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var args = new DiffViewEventArgs<Grid>(sender as Grid, container);
            LocationGridEventDispatcher.Instance.DispatchMouseWheelEvent(args, e);
        }

        private void DataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var args = new DiffViewEventArgs<FastGridControl>(sender as FastGridControl, container);
            DataGridEventDispatcher.Instance.DispatchSizeChangeEvent(args, e);
        }

        private void LocationGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var args = new DiffViewEventArgs<Grid>(sender as Grid, container);
            LocationGridEventDispatcher.Instance.DispatchSizeChangeEvent(args, e);
        }

        private void DataGrid_SelectedCellsChanged(object sender, FastWpfGrid.SelectionChangedEventArgs e)
        {
            var grid = copyTargetGrid = sender as FastGridControl;
            if (grid == null)
                return;

            copyTargetGrid = grid;

            var args = new DiffViewEventArgs<FastGridControl>(sender as FastGridControl, container);
            DataGridEventDispatcher.Instance.DispatchSelectedCellChangeEvent(args);

            if (!SrcDataGrid.CurrentCell.Row.HasValue || !DstDataGrid.CurrentCell.Row.HasValue)
                return;

            if (!SrcDataGrid.CurrentCell.Column.HasValue || !DstDataGrid.CurrentCell.Column.HasValue)
                return;

            if (SrcDataGrid.Model == null || DstDataGrid.Model == null)
                return;

            var srcValue =
                (SrcDataGrid.Model as DiffGridModel).GetCellText(SrcDataGrid.CurrentCell.Row.Value, SrcDataGrid.CurrentCell.Column.Value, true);
            var dstValue =
                (DstDataGrid.Model as DiffGridModel).GetCellText(DstDataGrid.CurrentCell.Row.Value, DstDataGrid.CurrentCell.Column.Value, true);

            UpdateValueDiff(srcValue, dstValue);

            if (App.Instance.Setting.AlwaysExpandCellDiff)
            {
                var a = new DiffViewEventArgs<RichTextBox>(null, container, TargetType.First);
                ValueTextBoxEventDispatcher.Instance.DispatchGotFocusEvent(a);
            }
        }

        private void ValueTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var args = new DiffViewEventArgs<RichTextBox>(sender as RichTextBox, container, TargetType.First);
            ValueTextBoxEventDispatcher.Instance.DispatchGotFocusEvent(args);
        }

        private void ValueTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var args = new DiffViewEventArgs<RichTextBox>(sender as RichTextBox, container, TargetType.First);
            ValueTextBoxEventDispatcher.Instance.DispatchLostFocusEvent(args);
        }

        private string GetRichTextString(RichTextBox textBox)
        {
            var textRange = new TextRange(textBox.Document.ContentStart, textBox.Document.ContentEnd);

            return textRange.Text;
        }

        private IEnumerable<DiffResult<string>> DiffCellValue(IEnumerable<string> src, IEnumerable<string> dst)
        {
            return DiffUtil.OptimizedDiff(src, dst);
        }

        private string ConvertWhiteSpaces(string str)
        {
            return new string(str.Select(c =>
            {
                if (Encoding.UTF8.GetByteCount(c.ToString()) == 1)
                    return ' ';
                else
                    return '　';

            }).ToArray());
        }

        private string ConvertWhiteSpaces(char c)
        {
            if (Encoding.UTF8.GetByteCount(c.ToString()) == 1)
                return " ";
            else
                return "　";
        }

        private void DiffModifiedLine(IEnumerable<DiffResult<char>> results, List<Tuple<string, Color?>> ranges, bool isSrc)
        {
            var splited = results.SplitByRegularity((items, current) => items.Last().Status.Equals(current.Status)).ToList();

            foreach (var sr in splited)
            {
                var status = sr.First().Status;
                if (status == DiffStatus.Equal)
                {
                    ranges.Add(Tuple.Create<string, Color?>(new string(sr.Select(r => r.Obj1).ToArray()), null));
                }
                else if (status == DiffStatus.Modified)
                {
                    var str = new string(sr.Select(r => isSrc ? r.Obj1 : r.Obj2).ToArray());
                    ranges.Add(Tuple.Create<string, Color?>(str, EMColor.LightOrange));
                }
                else if (status == DiffStatus.Deleted)
                {
                    var str = new string(sr.Select(r => r.Obj1).ToArray());
                    ranges.Add(Tuple.Create<string, Color?>(str, EMColor.LightGray));
                }
                else if (status == DiffStatus.Inserted)
                {
                    var str = new string(sr.Select(r => r.Obj2).ToArray());
                    ranges.Add(Tuple.Create<string, Color?>(str, EMColor.Orange));
                }
            }

            ranges.Add(Tuple.Create<string, Color?>("\n", null));
        }

        private void DiffEqualLine(DiffResult<string> lineDiffResult, List<Tuple<string, Color?>> ranges)
        {
            ranges.Add(Tuple.Create<string, Color?>(lineDiffResult.Obj1, null));
            ranges.Add(Tuple.Create<string, Color?>("\n", null));
        }

        private void DiffDeletedLine(DiffResult<string> lineDiffResult, List<Tuple<string, Color?>> ranges, bool isSrc)
        {
            var str = isSrc ? lineDiffResult.Obj1 : ConvertWhiteSpaces(lineDiffResult.Obj1.ToString());
            ranges.Add(Tuple.Create<string, Color?>(str, isSrc ? EMColor.LightGray : EMColor.LightGray));
            ranges.Add(Tuple.Create<string, Color?>("\n", null));
        }

        private void DiffInsertedLine(DiffResult<string> lineDiffResult, List<Tuple<string, Color?>> ranges, bool isSrc)
        {
            var str = isSrc ? ConvertWhiteSpaces(lineDiffResult.Obj2) : lineDiffResult.Obj2;
            ranges.Add(Tuple.Create<string, Color?>(str, isSrc ? EMColor.LightGray : EMColor.Orange));
            ranges.Add(Tuple.Create<string, Color?>("\n", null));
        }

        private void UpdateValueDiff(string srcValue, string dstValue)
        {
            SrcValueTextBox.Document.Blocks.FirstOrDefault()?.ContentStart.Paragraph.Inlines.Clear();
            DstValueTextBox.Document.Blocks.FirstOrDefault()?.ContentStart.Paragraph.Inlines.Clear();

            var srcLines = srcValue.Split('\n').Select(s => s.TrimEnd());
            var dstLines = dstValue.Split('\n').Select(s => s.TrimEnd());

            var lineDiffResults = DiffCellValue(srcLines, dstLines).ToList();

            var srcRange = new List<Tuple<string, Color?>>();
            var dstRange = new List<Tuple<string, Color?>>();
            foreach (var lineDiffResult in lineDiffResults)
            {
                if (lineDiffResult.Status == DiffStatus.Equal)
                {
                    DiffEqualLine(lineDiffResult, srcRange);
                    DiffEqualLine(lineDiffResult, dstRange);
                }
                else if (lineDiffResult.Status == DiffStatus.Modified)
                {
                    var charDiffResults = DiffUtil.OptimizedDiff(lineDiffResult.Obj1, lineDiffResult.Obj2);

                    DiffModifiedLine(charDiffResults.Where(r => r.Status != DiffStatus.Inserted), srcRange, true);
                    DiffModifiedLine(charDiffResults.Where(r => r.Status != DiffStatus.Deleted), dstRange, false);
                }
                else if (lineDiffResult.Status == DiffStatus.Deleted)
                {
                    DiffDeletedLine(lineDiffResult, srcRange, true);
                    DiffDeletedLine(lineDiffResult, dstRange, false);
                }
                else if (lineDiffResult.Status == DiffStatus.Inserted)
                {
                    DiffInsertedLine(lineDiffResult, srcRange, true);
                    DiffInsertedLine(lineDiffResult, dstRange, false);
                }
            }

            foreach (var r in srcRange)
            {
                var bc = r.Item2.HasValue ? new SolidColorBrush(r.Item2.Value) : new SolidColorBrush();
                SrcValueTextBox.Document.Blocks.First().ContentStart.Paragraph.Inlines.Add(new Run(r.Item1) { Background = bc });
            }

            foreach (var r in dstRange)
            {
                var bc = r.Item2.HasValue ? new SolidColorBrush(r.Item2.Value) : new SolidColorBrush();
                DstValueTextBox.Document.Blocks.First().ContentStart.Paragraph.Inlines.Add(new Run(r.Item1) { Background = bc });
            }
        }

        private void DiffButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteDiff();
        }

        private ExcelSheetReadConfig CreateReadConfig()
        {
            var setting = ((App)Application.Current).Setting;

            return new ExcelSheetReadConfig()
            {
                TrimFirstBlankRows = setting.SkipFirstBlankRows,
                TrimFirstBlankColumns = setting.SkipFirstBlankColumns,
                TrimLastBlankRows = setting.TrimLastBlankRows,
                TrimLastBlankColumns = setting.TrimLastBlankColumns,
            };
        }

        private Tuple<ExcelWorkbook, ExcelWorkbook> ReadWorkbooks()
        {
            var srcPath = Path.ChangeExtension(App.GetTempFileName(), Path.GetExtension(SrcPathTextBox.Text));
            var dstPath = Path.ChangeExtension(App.GetTempFileName(), Path.GetExtension(DstPathTextBox.Text));

            return ReadWorkbooks(srcPath, dstPath);
        }

        private Tuple<ExcelWorkbook, ExcelWorkbook> ReadWorkbooks(string srcPath, string dstPath)
        {
            ExcelWorkbook swb = null;
            ExcelWorkbook dwb = null;

            PathUtility.CopyTree(SrcPathTextBox.Text, srcPath, overwrite: true);
            File.SetAttributes(srcPath, FileAttributes.Normal);

            PathUtility.CopyTree(DstPathTextBox.Text, dstPath, overwrite: true);
            File.SetAttributes(dstPath, FileAttributes.Normal);

            ProgressWindow.DoWorkWithModal(progress =>
            {
                progress.Report(Properties.Resources.Msg_ReadingFiles);

                var config = CreateReadConfig();
                swb = ExcelWorkbook.Create(srcPath, config);
                dwb = ExcelWorkbook.Create(dstPath, config);
            });

            return Tuple.Create(swb, dwb);
        }

        private FileSetting FindFileSettings(bool isStartup)
        {
            FileSetting fileSetting = null;
            var srcPath = SrcPathTextBox.Text;
            var dstPath = DstPathTextBox.Text;
            if (!IgnoreFileSettingCheckbox.IsChecked.Value)
            {
                fileSetting =
                    FindFilseSetting(Path.GetFileName(srcPath), SrcSheetCombobox.SelectedIndex, SrcSheetCombobox.SelectedItem.ToString(), isStartup) ??
                    FindFilseSetting(Path.GetFileName(dstPath), DstSheetCombobox.SelectedIndex, DstSheetCombobox.SelectedItem.ToString(), isStartup);

                diffConfig = CreateDiffConfig(fileSetting, isStartup);
            }
            else
            {
                diffConfig = new ExcelSheetDiffConfig();

                diffConfig.SrcSheetIndex = Math.Max(SrcSheetCombobox.SelectedIndex, 0);
                diffConfig.DstSheetIndex = Math.Max(DstSheetCombobox.SelectedIndex, 0);
            }

            return fileSetting;
        }

        private ExcelSheetDiff ExecuteDiff(ExcelSheet srcSheet, ExcelSheet dstSheet)
        {
            ExcelSheetDiff diff = null;

            ProgressWindow.DoWorkWithModal(progress =>
            {
                progress.Report(Properties.Resources.Msg_ExtractingDiff);
                diff = ExcelSheet.Diff(srcSheet, dstSheet, diffConfig);
            });

            return diff;
        }

        private void CreateSheetDiff(bool isStartup, bool ignoreBatch = false)
        {
            var workbooks = ReadWorkbooks();
            var srcWorkbook = workbooks.Item1;
            var dstWorkbook = workbooks.Item2;

            if (!ignoreBatch && App.Instance.Setting.UseBatchExtraction)
            {
                foreach (var names in GetViewModel().GetSheetNamePairs())
                {
                    var srcSheet = srcWorkbook.Sheets.Values.FirstOrDefault(v => v.Name == names.Item1);
                    if (srcSheet == null)
                    {
                        srcSheet = ExcelSheet.CreateEmpty(names.Item1, srcWorkbook.Sheets.Max(s => s.Value.Index) + 1);
                        srcWorkbook.Sheets.Add(srcSheet.Name, srcSheet);
                    }

                    var dstSheet = dstWorkbook.Sheets.Values.FirstOrDefault(v => v.Name == names.Item2);
                    if (dstSheet == null)
                    {
                        dstSheet = ExcelSheet.CreateEmpty(names.Item2, dstWorkbook.Sheets.Max(s => s.Value.Index) + 1);
                        dstWorkbook.Sheets.Add(dstSheet.Name, dstSheet);
                    }

                    if (srcSheet.Rows.Count > 10000 || dstSheet.Rows.Count > 10000)
                        MessageBox.Show(Properties.Resources.Msg_WarnSize);

                    FindFileSettings(false);

                    var diff = ExecuteDiff(srcSheet, dstSheet);
                    GetViewModel().AddSheetDiff(diff);
                }
            }
            else
            {
                FindFileSettings(isStartup);

                SrcSheetCombobox.SelectedIndex = diffConfig.SrcSheetIndex;
                DstSheetCombobox.SelectedIndex = diffConfig.DstSheetIndex;

                var srcName = SrcSheetCombobox.SelectedItem.ToString();
                var srcSheet = srcWorkbook.Sheets.Values.FirstOrDefault(v => v.Name == srcName);
                if (srcSheet == null)
                {
                    srcSheet = ExcelSheet.CreateEmpty(srcName, srcWorkbook.Sheets.Max(s => s.Value.Index) + 1);
                    srcWorkbook.Sheets.Add(srcSheet.Name, srcSheet);
                }

                var dstName = DstSheetCombobox.SelectedItem.ToString();
                var dstSheet = dstWorkbook.Sheets.Values.FirstOrDefault(v => v.Name == dstName);
                if (dstSheet == null)
                {
                    dstSheet = ExcelSheet.CreateEmpty(dstName, dstWorkbook.Sheets.Max(s => s.Value.Index) + 1);
                    dstWorkbook.Sheets.Add(dstSheet.Name, dstSheet);
                }

                if (srcSheet.Rows.Count > 10000 || dstSheet.Rows.Count > 10000)
                    MessageBox.Show(Properties.Resources.Msg_WarnSize);

                var diff = ExecuteDiff(srcSheet, dstSheet);
                GetViewModel().AddSheetDiff(diff);
            }
        }

        private void ExecuteDiff(bool isStartup = false, bool useCache = false, ExcelSheetDiff specificSheetDiff = null)
        {
            if (!File.Exists(SrcPathTextBox.Text) || !File.Exists(DstPathTextBox.Text))
                return;

            SelectedSheetDiffCombobox.SelectedIndex = -1;
            SelectedSheetDiffCombobox.SelectedItem = null;

            var args = new DiffViewEventArgs<FastGridControl>(null, container, TargetType.First);
            DataGridEventDispatcher.Instance.DispatchPreExecuteDiffEvent(args);

            ExcelSheetDiff diff = specificSheetDiff;

            if (SrcSheetCombobox.SelectedIndex < 0 && SrcSheetCombobox.Items.Count > 0)
                SrcSheetCombobox.SelectedIndex = 0;

            if (DstSheetCombobox.SelectedIndex < 0 && DstSheetCombobox.Items.Count > 0)
                DstSheetCombobox.SelectedIndex = 0;

            if (diff == null)
            {
                var ignoreBatch = SrcSheetCombobox.SelectedIndex != DstSheetCombobox.SelectedIndex;

                if (!useCache)
                {
                    GetViewModel().SheetDiffs.Clear();
                    CreateSheetDiff(isStartup, ignoreBatch);
                }

                diff = GetViewModel().SheetDiffs
                    .FirstOrDefault(
                        s =>
                        GetViewModel().GetSrcSheetIndex(s.SrcSheet.Name) == diffConfig.SrcSheetIndex &&
                        GetViewModel().GetDstSheetIndex(s.DstSheet.Name) == diffConfig.DstSheetIndex);

                if (diff == null)
                {
                    if (useCache)
                    {
                        CreateSheetDiff(isStartup, ignoreBatch);
                        diff = GetViewModel().SheetDiffs
                            .FirstOrDefault(
                                s =>
                                GetViewModel().GetSrcSheetIndex(s.SrcSheet.Name) == diffConfig.SrcSheetIndex &&
                                GetViewModel().GetDstSheetIndex(s.DstSheet.Name) == diffConfig.DstSheetIndex);

                        if (diff == null)
                            return;
                    }
                    else
                    {
                        return;
                    }
                }
            }

            SrcDataGrid.Model = new DiffGridModel(diff, DiffType.Source);
            DstDataGrid.Model = new DiffGridModel(diff, DiffType.Dest);

            var fileSetting = FindFileSettings(isStartup);
            //var srcFileSetting = fileSettings.Item1;
            //var dstFileSetting = fileSettings.Item2;

            args = new DiffViewEventArgs<FastGridControl>(SrcDataGrid, container);
            DataGridEventDispatcher.Instance.DispatchFileSettingUpdateEvent(args, fileSetting);

            args = new DiffViewEventArgs<FastGridControl>(DstDataGrid, container);
            DataGridEventDispatcher.Instance.DispatchFileSettingUpdateEvent(args, fileSetting);

            args = new DiffViewEventArgs<FastGridControl>(null, container, TargetType.First);
            DataGridEventDispatcher.Instance.DispatchDisplayFormatChangeEvent(args, ShowOnlyDiffRadioButton.IsChecked.Value);
            DataGridEventDispatcher.Instance.DispatchPostExecuteDiffEvent(args);

            var summary = diff.CreateSummary();
            GetViewModel().UpdateDiffSummary(summary);

            if (!App.Instance.KeepFileHistory)
                App.Instance.UpdateRecentFiles(SrcPathTextBox.Text, DstPathTextBox.Text);

            if (App.Instance.Setting.NotifyEqual && !summary.HasDiff)
                MessageBox.Show(Properties.Resources.Message_NoDiff);

            if (App.Instance.Setting.FocusFirstDiff)
                MoveNextModifiedCell();
        }

        private FileSetting FindFilseSetting(string fileName, int sheetIndex, string sheetName, bool isStartup)
        {
            var results = new List<FileSetting>();
            foreach (var setting in App.Instance.Setting.FileSettings)
            {
                if (setting.UseRegex)
                {
                    var regex = new System.Text.RegularExpressions.Regex(setting.Name);

                    if (regex.IsMatch(fileName))
                        results.Add(setting);
                }
                else
                {
                    if (setting.ExactMatch)
                    {
                        if (setting.Name == fileName)
                            results.Add(setting);
                    }
                    else
                    {
                        if (fileName.Contains(setting.Name))
                            results.Add(setting);
                    }
                }
            }

            if (isStartup)
                return results.FirstOrDefault(r => r.IsStartupSheet) ?? results.FirstOrDefault() ?? null;

            return results.FirstOrDefault(r => r.SheetName == sheetName) ?? results.FirstOrDefault(r => r.SheetIndex == sheetIndex) ?? null;
        }

        private void SetRowHeader_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                var dataGrid = ((ContextMenu)menuItem.Parent).PlacementTarget as FastGridControl;
                if (dataGrid != null)
                {
                    var args = new DiffViewEventArgs<FastGridControl>(dataGrid, container, TargetType.First);
                    DataGridEventDispatcher.Instance.DispatchRowHeaderChagneEvent(args);
                }
            }
        }

        private void ResetRowHeader_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                var dataGrid = ((ContextMenu)menuItem.Parent).PlacementTarget as FastGridControl;
                if (dataGrid != null)
                {
                    var args = new DiffViewEventArgs<FastGridControl>(dataGrid, container, TargetType.First);
                    DataGridEventDispatcher.Instance.DispatchRowHeaderResetEvent(args);
                }
            }
        }

        private void SetColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                var dataGrid = ((ContextMenu)menuItem.Parent).PlacementTarget as FastGridControl;
                if (dataGrid != null)
                {
                    var args = new DiffViewEventArgs<FastGridControl>(dataGrid, container, TargetType.First);
                    DataGridEventDispatcher.Instance.DispatchColumnHeaderChangeEvent(args);
                }
            }
        }

        private void ResetColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                var dataGrid = ((ContextMenu)menuItem.Parent).PlacementTarget as FastGridControl;
                if (dataGrid != null)
                {
                    var args = new DiffViewEventArgs<FastGridControl>(dataGrid, container, TargetType.First);
                    DataGridEventDispatcher.Instance.DispatchColumnHeaderResetEvent(args);
                }
            }
        }

        private void SwapButton_Click(object sender, RoutedEventArgs e)
        {
            Swap();
        }

        private void Swap()
        {
            var srcTmp = SrcSheetCombobox.SelectedIndex;
            var dstTmp = DstSheetCombobox.SelectedIndex;

            var tmp = SrcPathTextBox.Text;
            SrcPathTextBox.Text = DstPathTextBox.Text;
            DstPathTextBox.Text = tmp;

            GetViewModel().SrcPath = SrcPathTextBox.Text;
            GetViewModel().DstPath = DstPathTextBox.Text;

            diffConfig.SrcSheetIndex = dstTmp;
            diffConfig.DstSheetIndex = srcTmp;

            ExecuteDiff();
        }

        private void DiffByHeaderSrc_Click(object sender, RoutedEventArgs e)
        {
            var headerIndex = SrcDataGrid.CurrentCell.Row.HasValue ? SrcDataGrid.CurrentCell.Row.Value : -1;

            diffConfig.SrcHeaderIndex= headerIndex;

            ExecuteDiff();
        }

        private void DiffByHeaderDst_Click(object sender, RoutedEventArgs e)
        {
            var headerIndex = DstDataGrid.CurrentCell.Row.HasValue ? DstDataGrid.CurrentCell.Row.Value : -1;

            diffConfig.DstSheetIndex = headerIndex;

            ExecuteDiff();
        }

        private void ShowAllRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var args = new DiffViewEventArgs<FastGridControl>(null, container, TargetType.First);
            DataGridEventDispatcher.Instance.DispatchDisplayFormatChangeEvent(args, false);
        }

        private void ShowOnlyDiffRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var args = new DiffViewEventArgs<FastGridControl>(null, container, TargetType.First);
            DataGridEventDispatcher.Instance.DispatchDisplayFormatChangeEvent(args, true);
        }

        private bool ValidateDataGrids()
        {
            return SrcDataGrid.Model != null && DstDataGrid.Model != null;
        }

        private void ValuteTextBox_ScrollChanged(object sender, RoutedEventArgs e)
        {
            var args = new DiffViewEventArgs<RichTextBox>(sender as RichTextBox, container);
            ValueTextBoxEventDispatcher.Instance.DispatchScrolledEvent(args, (ScrollChangedEventArgs)e);
        }

        private void NextModifiedCellButton_Click(object sender, RoutedEventArgs e)
        {
            MoveNextModifiedCell();
        }

        private void MoveNextModifiedCell()
        {
            if (!ValidateDataGrids())
                return;

            var nextCell = (SrcDataGrid.Model as DiffGridModel).GetNextModifiedCell(
                SrcDataGrid.CurrentCell.IsEmpty ? FastGridCellAddress.Zero : SrcDataGrid.CurrentCell);
            if (nextCell.IsEmpty)
                return;

            SrcDataGrid.CurrentCell = nextCell;
        }

        private void PrevModifiedCellButton_Click(object sender, RoutedEventArgs e)
        {
            MovePrevModifiedCell();
        }

        private void MovePrevModifiedCell()
        {
            if (!ValidateDataGrids())
                return;

            var nextCell = (SrcDataGrid.Model as DiffGridModel).GetPreviousModifiedCell(
                SrcDataGrid.CurrentCell.IsEmpty ? FastGridCellAddress.Zero : SrcDataGrid.CurrentCell);
            if (nextCell.IsEmpty)
                return;

            SrcDataGrid.CurrentCell = nextCell;
        }

        private void NextModifiedRowButton_Click(object sender, RoutedEventArgs e)
        {
            MoveNextModifiedRow();
        }

        private void MoveNextModifiedRow()
        {
            if (!ValidateDataGrids())
                return;

            var nextCell = (SrcDataGrid.Model as DiffGridModel).GetNextModifiedRow(
                SrcDataGrid.CurrentCell.IsEmpty ? FastGridCellAddress.Zero : SrcDataGrid.CurrentCell);
            if (nextCell.IsEmpty)
                return;

            SrcDataGrid.CurrentCell = nextCell;
        }

        private void PrevModifiedRowButton_Click(object sender, RoutedEventArgs e)
        {
            MovePrevModifiedRow();
        }

        private void MovePrevModifiedRow()
        {
            if (!ValidateDataGrids())
                return;

            var nextCell = (SrcDataGrid.Model as DiffGridModel).GetPreviousModifiedRow(
                SrcDataGrid.CurrentCell.IsEmpty ? FastGridCellAddress.Zero : SrcDataGrid.CurrentCell);
            if (nextCell.IsEmpty)
                return;

            SrcDataGrid.CurrentCell = nextCell;
        }

        private void NextAddedRowButton_Click(object sender, RoutedEventArgs e)
        {
            MoveNextAddedRow();
        }

        private void MoveNextAddedRow()
        {
            if (!ValidateDataGrids())
                return;

            var nextCell = (SrcDataGrid.Model as DiffGridModel).GetNextAddedRow(
                SrcDataGrid.CurrentCell.IsEmpty ? FastGridCellAddress.Zero : SrcDataGrid.CurrentCell);
            if (nextCell.IsEmpty)
                return;

            SrcDataGrid.CurrentCell = nextCell;
        }

        private void PrevAddedRowButton_Click(object sender, RoutedEventArgs e)
        {
            MovePrevAddedRow();
        }

        private void MovePrevAddedRow()
        {
            if (!ValidateDataGrids())
                return;

            var nextCell = (SrcDataGrid.Model as DiffGridModel).GetPreviousAddedRow(
                SrcDataGrid.CurrentCell.IsEmpty ? FastGridCellAddress.Zero : SrcDataGrid.CurrentCell);
            if (nextCell.IsEmpty)
                return;

            SrcDataGrid.CurrentCell = nextCell;
        }

        private void NextRemovedRowButton_Click(object sender, RoutedEventArgs e)
        {
            MoveNextRemovedRow();
        }

        private void MoveNextRemovedRow()
        {
            if (!ValidateDataGrids())
                return;

            var nextCell = (SrcDataGrid.Model as DiffGridModel).GetNextRemovedRow(
                SrcDataGrid.CurrentCell.IsEmpty ? FastGridCellAddress.Zero : SrcDataGrid.CurrentCell);
            if (nextCell.IsEmpty)
                return;

            SrcDataGrid.CurrentCell = nextCell;
        }

        private void PrevRemovedRowButton_Click(object sender, RoutedEventArgs e)
        {
            MovePrevRemovedRow();
        }

        private void MovePrevRemovedRow()
        {
            if (!ValidateDataGrids())
                return;

            var nextCell = (SrcDataGrid.Model as DiffGridModel).GetPreviousRemovedRow(
                SrcDataGrid.CurrentCell.IsEmpty ? FastGridCellAddress.Zero : SrcDataGrid.CurrentCell);
            if (nextCell.IsEmpty)
                return;

            SrcDataGrid.CurrentCell = nextCell;
        }

        private void PrevMatchCellButton_Click(object sender, RoutedEventArgs e)
        {
            MovePrevMatchCell();
        }

        private void MovePrevMatchCell()
        {
            if (!ValidateDataGrids())
                return;

            var text = SearchTextCombobox.Text;
            if (string.IsNullOrEmpty(text))
                return;

            var history = App.Instance.Setting.SearchHistory.ToList();
            if (history.Contains(text))
                history.Remove(text);

            history.Insert(0, text);
            history = history.Take(10).ToList();

            App.Instance.Setting.SearchHistory = new ObservableCollection<string>(history);
            App.Instance.Setting.Save();

            SearchTextCombobox.ItemsSource = App.Instance.Setting.SearchHistory.ToList();

            var nextCell = (SrcDataGrid.Model as DiffGridModel).GetPreviousMatchCell(
                SrcDataGrid.CurrentCell.IsEmpty ? FastGridCellAddress.Zero : SrcDataGrid.CurrentCell, text,
                ExactMatchCheckBox.IsChecked.Value, CaseSensitiveCheckBox.IsChecked.Value, RegexCheckBox.IsChecked.Value, ShowOnlyDiffRadioButton.IsChecked.Value);
            if (nextCell.IsEmpty)
                return;

            SrcDataGrid.CurrentCell = nextCell;
        }

        private void NextMatchCellButton_Click(object sender, RoutedEventArgs e)
        {
            MoveNextMatchCell();
        }

        private void MoveNextMatchCell()
        {
            if (!ValidateDataGrids())
                return;

            var text = SearchTextCombobox.Text;
            if (string.IsNullOrEmpty(text))
                return;

            var history = App.Instance.Setting.SearchHistory.ToList();
            if (history.Contains(text))
                history.Remove(text);

            history.Insert(0, text);
            history = history.Take(10).ToList();

            App.Instance.Setting.SearchHistory = new ObservableCollection<string>(history);
            App.Instance.Setting.Save();

            SearchTextCombobox.ItemsSource = App.Instance.Setting.SearchHistory.ToList();

            var nextCell = (SrcDataGrid.Model as DiffGridModel).GetNextMatchCell(
                SrcDataGrid.CurrentCell.IsEmpty ? FastGridCellAddress.Zero : SrcDataGrid.CurrentCell, text,
                ExactMatchCheckBox.IsChecked.Value, CaseSensitiveCheckBox.IsChecked.Value, RegexCheckBox.IsChecked.Value, ShowOnlyDiffRadioButton.IsChecked.Value);
            if (nextCell.IsEmpty)
                return;

            SrcDataGrid.CurrentCell = nextCell;
        }

        private void CopyToClipboardSelectedCells(string separator)
        {
            if (copyTargetGrid == null)
                return;

            var model = copyTargetGrid.Model as DiffGridModel;
            if (model == null)
                return;

            var tsv = string.Join(Environment.NewLine,
               copyTargetGrid.SelectedCells
              .GroupBy(c => c.Row.Value)
              .OrderBy(g => g.Key)
              .Select(g => string.Join(separator, g.Select(c => model.GetCellText(c, true)))));

            Clipboard.SetDataObject(tsv);
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Right:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            MoveNextModifiedCell();
                            e.Handled = true;
                        }
                    }
                    break;
                case Key.Left:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            MovePrevModifiedCell();
                            e.Handled = true;
                        }
                    }
                    break;
                case Key.Down:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            MoveNextModifiedRow();
                            e.Handled = true;
                        }
                    }
                    break;
                case Key.Up:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            MovePrevModifiedRow();
                            e.Handled = true;
                        }
                    }
                    break;
                case Key.L:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            MoveNextRemovedRow();
                            e.Handled = true;
                        }
                    }
                    break;
                case Key.O:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            MovePrevRemovedRow();
                            e.Handled = true;
                        }
                    }
                    break;
                case Key.K:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            MoveNextAddedRow();
                            e.Handled = true;
                        }
                    }
                    break;
                case Key.I:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            MovePrevAddedRow();
                            e.Handled = true;
                        }
                    }
                    break;
                case Key.F8:
                    {
                        MovePrevMatchCell();
                        e.Handled = true;
                    }
                    break;
                case Key.F9:
                    {
                        MoveNextMatchCell();
                        e.Handled = true;
                    }
                    break;
                case Key.F:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            ToolExpander.IsExpanded = true;
                            SearchTextCombobox.Focus();
                            e.Handled = true;
                        }
                    }
                    break;
                case Key.C:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            CopyToClipboardSelectedCells(Keyboard.IsKeyDown(Key.RightShift) || Keyboard.IsKeyDown(Key.LeftShift) ? "," : "\t");
                            e.Handled = true;
                        }
                    }
                    break;
                case Key.B:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            ShowLog();
                            e.Handled = true;
                        }
                    }
                    break;
            }
        }

        private void ShowLog()
        {
            var log = BuildCellBaseLog();

            (App.Current.MainWindow as MainWindow).WriteToConsole(log);
        }

        private void BuildCellBaseLog_Click(object sender, RoutedEventArgs e)
        {
            ShowLog();
        }

        private string BuildCellBaseLog()
        {
            var srcModel = SrcDataGrid.Model as DiffGridModel;
            if (srcModel == null)
                return string.Empty;

            var dstModel = DstDataGrid.Model as DiffGridModel;
            if (dstModel == null)
                return string.Empty;

            var builder = new StringBuilder();

            var selectedCells = SrcDataGrid.SelectedCells;

            var modifiedLogFormat = App.Instance.Setting.LogFormat;
            var addedLogFormat = App.Instance.Setting.AddedRowLogFormat;
            var removedLogFormat = App.Instance.Setting.RemovedRowLogFormat;

            foreach (var row in SrcDataGrid.SelectedCells.GroupBy(c => c.Row))
            {
                var rowHeaderText = srcModel.GetRowHeaderText(row.Key.Value);
                if (string.IsNullOrEmpty(rowHeaderText))
                    rowHeaderText = dstModel.GetRowHeaderText(row.Key.Value);

                if (dstModel.IsAddedRow(row.Key.Value, true))
                {
                    var log = addedLogFormat
                        .Replace("${ROW}", RemoveMultiLine(rowHeaderText));

                    builder.AppendLine(log);

                    continue;
                }

                if (dstModel.IsRemovedRow(row.Key.Value, true))
                {
                    var log = removedLogFormat
                        .Replace("${ROW}", RemoveMultiLine(rowHeaderText));

                    builder.AppendLine(log);

                    continue;
                }

                foreach (var cell in row)
                {
                    if (cell.Row.Value == srcModel.ColumnHeaderIndex)
                        continue;

                    var srcText = srcModel.GetCellText(cell, true);
                    var dstText = dstModel.GetCellText(cell, true);
                    if (srcText == dstText)
                        continue;

                    var colHeaderText = srcModel.GetColumnHeaderText(cell.Column.Value);

                    if (string.IsNullOrEmpty(colHeaderText))
                        colHeaderText = dstModel.GetColumnHeaderText(cell.Column.Value);

                    if (string.IsNullOrEmpty(srcText))
                        srcText = Properties.Resources.Word_Blank;

                    if (string.IsNullOrEmpty(dstText))
                        dstText = Properties.Resources.Word_Blank;

                    if (string.IsNullOrEmpty(rowHeaderText))
                        rowHeaderText = Properties.Resources.Word_Blank;

                    if (string.IsNullOrEmpty(colHeaderText))
                        colHeaderText = Properties.Resources.Word_Blank;

                    var log = modifiedLogFormat
                        .Replace("${ROW}", RemoveMultiLine(rowHeaderText))
                        .Replace("${COL}", RemoveMultiLine(colHeaderText))
                        .Replace("${LEFT}", RemoveMultiLine(srcText))
                        .Replace("${RIGHT}", RemoveMultiLine(dstText));

                    builder.AppendLine(log);
                }
            }

            return builder.ToString();
        }

        private string RemoveMultiLine(string log)
        {
            return log.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
        }

        private void CopyAsTsv_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboardSelectedCells("\t");
        }

        private void CopyAsCsv_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboardSelectedCells(",");
        }

        private void PrevSheetButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPrevSheetDiff();
        }

        private void NextSheetButton_Click(object sender, RoutedEventArgs e)
        {
            ShowNextSheetDiff();
        }

        private void SelectedSheetDiffCombobox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var diff = ((sender as ComboBox).SelectedItem) as ExcelSheetDiff;
            if (diff == null)
                return;

            SrcSheetCombobox.SelectedItem = diff.SrcSheet.Name;
            DstSheetCombobox.SelectedItem = diff.DstSheet.Name;

            ExecuteDiff(specificSheetDiff: diff);
        }
    }
}
