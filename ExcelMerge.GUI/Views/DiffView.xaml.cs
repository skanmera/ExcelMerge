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
using SKCore.Collection;
using ExcelMerge.GUI.ViewModels;
using ExcelMerge.GUI.Models;
using ExcelMerge.GUI.Styles;

namespace ExcelMerge.GUI.Views
{
    public partial class DiffView : UserControl
    {
        private ExcelSheetDiffConfig diffConfig;
        private IUnityContainer container;

        private const string srcKey = "src";
        private const string dstKey = "dst";

        public DiffView()
        {
            InitializeComponent();

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

            var srcEventHandler = new EventHandler(srcKey);
            var dstEventHandler = new EventHandler(dstKey);

            DataGridEventDispatcher.Listeners.Add(srcEventHandler);
            DataGridEventDispatcher.Listeners.Add(dstEventHandler);
            LocationGridEventDispatcher.Listeners.Add(srcEventHandler);
            LocationGridEventDispatcher.Listeners.Add(dstEventHandler);
            ViewportEventDispatcher.Listeners.Add(srcEventHandler);
            ViewportEventDispatcher.Listeners.Add(dstEventHandler);
            ValueTextBoxEventDispatcher.Listeners.Add(srcEventHandler);
            ValueTextBoxEventDispatcher.Listeners.Add(dstEventHandler);

            App.Instance.OnSettingUpdated += () =>
            {
                SrcDataGrid.AlternatingColors = App.Instance.Setting.AlternatingColors;
                SrcDataGrid.CellFontName = App.Instance.Setting.FontName;
                DataGridEventDispatcher.DispatchModelUpdateEvent(SrcDataGrid, container);
                DstDataGrid.AlternatingColors = App.Instance.Setting.AlternatingColors;
                DstDataGrid.CellFontName = App.Instance.Setting.FontName;
                DataGridEventDispatcher.DispatchModelUpdateEvent(DstDataGrid, container);
            };

            SearchTextCombobox.ItemsSource = App.Instance.Setting.SearchHistory.ToList();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            diffConfig = FindConfig();

            ExecuteDiff();

            SrcDataGrid.ScrolledModelRows += DataGrid_Scrolled;
            SrcDataGrid.ScrolledModelColumns += DataGrid_Scrolled;
            DstDataGrid.ScrolledModelRows += DataGrid_Scrolled;
            DstDataGrid.ScrolledModelColumns += DataGrid_Scrolled;
        }

        private ExcelSheetDiffConfig FindConfig()
        {
            var config = new ExcelSheetDiffConfig();

            return config;
        }

        private void DataGrid_Scrolled(object sender, EventArgs e)
        {
            DataGridEventDispatcher.DispatchScrollEvnet(sender as FastGridControl, container);
        }

        private void LocationGrid_MouseDown(object sender, MouseEventArgs e)
        {
            LocationGridEventDispatcher.DispatchMouseDownEvent(sender as Grid, container, e);
        }

        private void LocationGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                LocationGridEventDispatcher.DispatchMouseDownEvent(sender as Grid, container, e);
        }

        private void LocationGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            LocationGridEventDispatcher.DispatchMouseWheelEvent(sender as Grid, container, e);
        }

        private void DataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DataGridEventDispatcher.DispatchSizeChangeEvent(sender as FastGridControl, container, e);
        }

        private void LocationGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LocationGridEventDispatcher.DispatchSizeChangeEvent(SrcLocationGrid, container, e);
        }

        private void DataGrid_SelectedCellsChanged(object sender, FastWpfGrid.SelectionChangedEventArgs e)
        {
            DataGridEventDispatcher.DispatchSelectedCellChangeEvent(sender as FastGridControl, container);

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
        }

        private void ValueTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ValueTextBoxEventDispatcher.DispatchGotFocusEvent(sender as RichTextBox, container);
        }

        private void ValueTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ValueTextBoxEventDispatcher.DispatchLostFocusEvent(sender as RichTextBox, container);
        }

        private string GetRichTextString(RichTextBox textBox)
        {
            var textRange = new TextRange(textBox.Document.ContentStart, textBox.Document.ContentEnd);

            return textRange.Text;
        }

        private IEnumerable<DiffResult<string>> DiffCellValue(IEnumerable<string> src, IEnumerable<string> dst)
        {
            var option = DiffOption<string>.Default;
            option.Order = DiffOrder.LazyDeleteFirst;

            return DiffUtil.OptimizeCaseDeletedFirst(DiffUtil.Diff(src, dst, option));
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
            SrcValueTextBox.Document.Blocks.First().ContentStart.Paragraph.Inlines.Clear();
            DstValueTextBox.Document.Blocks.First().ContentStart.Paragraph.Inlines.Clear();

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
                    var opt = DiffOption<char>.Default;
                    opt.Order = DiffOrder.LazyDeleteFirst;
                    var charDiffResults = DiffUtil.OptimizeCaseDeletedFirst(DiffUtil.Diff(lineDiffResult.Obj1, lineDiffResult.Obj2, opt)).ToList();

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

        private void ExecuteDiff()
        {
            if (!File.Exists(SrcPathTextBox.Text) || !File.Exists(DstPathTextBox.Text))
                return;

            SrcDataGrid.ScrollIntoView(FastGridCellAddress.Empty);
            DstDataGrid.ScrollIntoView(FastGridCellAddress.Empty);

            SrcDataGrid.FirstVisibleColumnScrollIndex = 0;
            SrcDataGrid.FirstVisibleRowScrollIndex = 0;
            DstDataGrid.FirstVisibleColumnScrollIndex = 0;
            DstDataGrid.FirstVisibleRowScrollIndex = 0;

            SrcDataGrid.InitializeComponent();
            DstDataGrid.InitializeComponent();

            SrcDataGrid.SetMaxColumnSize(App.Instance.Setting.CellWidth);
            DstDataGrid.SetMaxColumnSize(App.Instance.Setting.CellWidth);
            SrcDataGrid.SetMinColumnSize(App.Instance.Setting.CellWidth);
            DstDataGrid.SetMinColumnSize(App.Instance.Setting.CellWidth);

            var config = CreateReadConfig();
            var wb1 = ExcelWorkbook.Create(SrcPathTextBox.Text, config);
            var wb2 = ExcelWorkbook.Create(DstPathTextBox.Text, config);

            var tmpSrcSheetIndex = diffConfig.SrcSheetIndex;
            var tmpDstSheetIndex = diffConfig.DstSheetIndex;

            SrcSheetCombobox.Items.Clear();
            DstSheetCombobox.Items.Clear();

            wb1.Sheets.Keys.ToList().ForEach(s => SrcSheetCombobox.Items.Add(s));
            wb2.Sheets.Keys.ToList().ForEach(s => DstSheetCombobox.Items.Add(s));

            SrcSheetCombobox.SelectedIndex = Math.Max(tmpSrcSheetIndex, 0);
            DstSheetCombobox.SelectedIndex = Math.Max(tmpDstSheetIndex, 0);

            var sheet1 = wb1.Sheets[SrcSheetCombobox.SelectedItem.ToString()];
            var sheet2 = wb2.Sheets[DstSheetCombobox.SelectedItem.ToString()];

            var diff = ExcelSheet.Diff(sheet1, sheet2, diffConfig);

            var modelConfig = new DiffGridModelConfig();
            var srcModel = new DiffGridModel(DiffType.Source, diff, modelConfig);
            var dstModel = new DiffGridModel(DiffType.Dest, diff, modelConfig);

            (DataContext as ViewModels.DiffViewModel).UpdateDiffSummary(diff.CreateSummary());

            SrcDataGrid.Model = srcModel;
            DstDataGrid.Model = dstModel;

            if (ShowOnlyDiffRadioButton.IsChecked.Value)
            {
                srcModel.HideEqualRows();
                srcModel.HideEqualRows();
            }

            var setting = FindFilseSetting(Path.GetFileName(SrcPathTextBox.Text)) ?? FindFilseSetting(Path.GetFileName(DstPathTextBox.Text));
            if (setting != null)
            {
                srcModel.SetColumnHeader(setting.ColumnHeaderIndex);
                dstModel.SetColumnHeader(setting.ColumnHeaderIndex);
                srcModel.SetRowHeader(setting.RowHeaderIndex);
                dstModel.SetRowHeader(setting.RowHeaderIndex);
                SrcDataGrid.MaxRowHeaderWidth = setting.MaxRowHeaderWidth;
                DstDataGrid.MaxRowHeaderWidth = setting.MaxRowHeaderWidth;
            }

            DataGridEventDispatcher.DispatchModelUpdateEvent(SrcDataGrid, container);
            DataGridEventDispatcher.DispatchModelUpdateEvent(DstDataGrid, container);

            if (!App.Instance.KeepFileHistory)
                App.Instance.UpdateRecentFiles(SrcPathTextBox.Text, DstPathTextBox.Text);

            SrcDataGrid.AlternatingColors = App.Instance.Setting.AlternatingColors;
            DstDataGrid.AlternatingColors = App.Instance.Setting.AlternatingColors;
            SrcDataGrid.CellFontName = App.Instance.Setting.FontName;
            DstDataGrid.CellFontName = App.Instance.Setting.FontName;
        }

        private Settings.FileSetting FindFilseSetting(string fileName)
        {
            foreach (var setting in App.Instance.Setting.FileSettings)
            {
                if (setting.UseRegex)
                {
                    var regex = new System.Text.RegularExpressions.Regex(setting.Name);
                    if (regex.IsMatch(fileName))
                        return setting;
                }
                else
                {
                    if (setting.ExactMatch)
                    {
                        if (setting.Name == fileName)
                            return setting;
                    }
                    else
                    {
                        if (fileName.Contains(setting.Name))
                            return setting;
                    }
                }
            }

            return null;
        }

        private void SetRowHeader_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                var dataGrid = ((ContextMenu)menuItem.Parent).PlacementTarget as FastGridControl;
                if (dataGrid != null)
                    DataGridEventDispatcher.DispatchRowHeaderChagneEvent(dataGrid, container);
            }
        }

        private void ResetRowHeader_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                var dataGrid = ((ContextMenu)menuItem.Parent).PlacementTarget as FastGridControl;
                if (dataGrid != null)
                    DataGridEventDispatcher.DispatchRowHeaderResetEvent(sender as FastGridControl, container);
            }
        }

        private void SetColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                var dataGrid = ((ContextMenu)menuItem.Parent).PlacementTarget as FastGridControl;
                if (dataGrid != null)
                    DataGridEventDispatcher.DispatchColumnHeaderChangeEvent(dataGrid, container);
            }
        }

        private void ResetColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                var dataGrid = ((ContextMenu)menuItem.Parent).PlacementTarget as FastGridControl;
                if (dataGrid != null)
                    DataGridEventDispatcher.DispatchColumnHeaderResetEvent(sender as FastGridControl, container);
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

            diffConfig.SrcSheetIndex = dstTmp;
            diffConfig.DstSheetIndex = srcTmp;

            ExecuteDiff();
        }

        private void SrcPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SrcSheetCombobox.Items.Clear();
        }

        private void DstPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            DstSheetCombobox.Items.Clear();
        }

        private void SrcSheetCombobox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (diffConfig == null)
                return;

            diffConfig.SrcSheetIndex = Math.Max(SrcSheetCombobox.SelectedIndex, 0);
        }

        private void DstSheetCombobox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (diffConfig == null)
                return;

            diffConfig.DstSheetIndex = Math.Max(DstSheetCombobox.SelectedIndex, 0);
        }

        private void DiffByHeaderSrc_Click(object sender, RoutedEventArgs e)
        {
            var headerIndex = SrcDataGrid.CurrentCell.Row.HasValue ? SrcDataGrid.CurrentCell.Row.Value : -1;

            diffConfig.HeaderIndex = headerIndex;

            ExecuteDiff();
        }

        private void DiffByHeaderDst_Click(object sender, RoutedEventArgs e)
        {
            var headerIndex = DstDataGrid.CurrentCell.Row.HasValue ? DstDataGrid.CurrentCell.Row.Value : -1;

            diffConfig.HeaderIndex = headerIndex;

            ExecuteDiff();
        }

        private void ShowAllRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (SrcDataGrid?.Model != null && DstDataGrid?.Model != null)
            {
                (SrcDataGrid.Model as DiffGridModel).ShowEqualRows();
                DataGridEventDispatcher.DispatchModelUpdateEvent(SrcDataGrid, container);

                (DstDataGrid.Model as DiffGridModel).ShowEqualRows();
                DataGridEventDispatcher.DispatchModelUpdateEvent(DstDataGrid, container);
            }
        }

        private void ShowOnlyDiffRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (SrcDataGrid?.Model != null && DstDataGrid?.Model != null)
            {
                (SrcDataGrid.Model as DiffGridModel).HideEqualRows();
                DataGridEventDispatcher.DispatchModelUpdateEvent(SrcDataGrid, container);

                (DstDataGrid.Model as DiffGridModel).HideEqualRows();
                DataGridEventDispatcher.DispatchModelUpdateEvent(DstDataGrid, container);
            }
        }

        private bool ValidateDataGrids()
        {
            return SrcDataGrid.Model != null && DstDataGrid.Model != null;
        }

        private void ValuteTextBox_ScrollChanged(object sender, RoutedEventArgs e)
        {
            ValueTextBoxEventDispatcher.DispatchScrolledEvent(sender as RichTextBox, container, (ScrollChangedEventArgs)e);
        }

        private void NextModifiedCellButton_Click(object sender, RoutedEventArgs e)
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
                ExactMatchCheckBox.IsChecked.Value, CaseSensitiveCheckBox.IsChecked.Value, RegexCheckBox.IsChecked.Value);
            if (nextCell.IsEmpty)
                return;

            SrcDataGrid.CurrentCell = nextCell;
        }

        private void NextMatchCellButton_Click(object sender, RoutedEventArgs e)
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
                ExactMatchCheckBox.IsChecked.Value, CaseSensitiveCheckBox.IsChecked.Value, RegexCheckBox.IsChecked.Value);
            if (nextCell.IsEmpty)
                return;

            SrcDataGrid.CurrentCell = nextCell;
        }
    }
}
