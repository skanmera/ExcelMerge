using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace FastWpfGrid
{
    partial class FastGridControl
    {
        #region property Model

        public IFastGridModel Model
        {
            get { return (IFastGridModel)this.GetValue(ModelProperty); }
            set { this.SetValue(ModelProperty, value); }
        }

        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(
            "Model", typeof(IFastGridModel), typeof(FastGridControl), new PropertyMetadata(null, OnModelPropertyChanged));

        private static void OnModelPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl)dependencyObject).OnModelPropertyChanged();
        }

        #endregion

        #region property IsTransposed

        public bool IsTransposed
        {
            get { return (bool)this.GetValue(IsTransposedProperty); }
            set { this.SetValue(IsTransposedProperty, value); }
        }

        public static readonly DependencyProperty IsTransposedProperty = DependencyProperty.Register(
            "IsTransposed", typeof(bool), typeof(FastGridControl), new PropertyMetadata(false, OnIsTransposedPropertyChanged));

        private static void OnIsTransposedPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl)dependencyObject).OnIsTransposedPropertyChanged();
        }

        #endregion

        #region property UseClearType

        public bool UseClearType
        {
            get { return (bool)this.GetValue(UseClearTypeProperty); }
            set { this.SetValue(UseClearTypeProperty, value); }
        }

        public static readonly DependencyProperty UseClearTypeProperty = DependencyProperty.Register(
            "UseClearType", typeof(bool), typeof(FastGridControl), new PropertyMetadata(true, OnUseClearTypePropertyChanged));

        private static void OnUseClearTypePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl)dependencyObject).OnUseClearTypePropertyChanged();
        }

        #endregion

        #region property AllowFlexibleRows

        public bool AllowFlexibleRows
        {
            get { return (bool)this.GetValue(AllowFlexibleRowsProperty); }
            set { this.SetValue(AllowFlexibleRowsProperty, value); }
        }

        public static readonly DependencyProperty AllowFlexibleRowsProperty = DependencyProperty.Register(
            "AllowFlexibleRows", typeof(bool), typeof(FastGridControl), new PropertyMetadata(false, OnAllowFlexibleRowsPropertyChanged));

        private static void OnAllowFlexibleRowsPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl)dependencyObject).OnAllowFlexibleRowsPropertyChanged();
        }

        #endregion

        #region AlternatingColors

        public Color[] AlternatingColors
        {
            get { return (Color[])this.GetValue(AlternatingColorsProperty); }
            set { this.SetValue(AlternatingColorsProperty, value); }
        }

        public static readonly DependencyProperty AlternatingColorsProperty = DependencyProperty.Register(
            "AlternatingColors", typeof(Color[]), typeof(FastGridControl), new PropertyMetadata(null, OnAlternatingColorsPropertyChanged));

        private static void OnAlternatingColorsPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl)dependencyObject).OnAlternatingColorsChanged();
        }

        #endregion

        #region FontName

        public string CellFontName
        {
            get { return (string)this.GetValue(CellFontNameProperty); }
            set { this.SetValue(CellFontNameProperty, value); }
        }

        public static readonly DependencyProperty CellFontNameProperty = DependencyProperty.Register(
            "CellFontName", typeof(string), typeof(FastGridControl), new PropertyMetadata("Arial", OnCellFontNamePropertyChanged));

        private static void OnCellFontNamePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl)dependencyObject).OnCellFontNameChanged();
        }

        #endregion

        #region CellFontSize

        public int CellFontSize
        {
            get { return (int)this.GetValue(CellFontSizeProperty); }
            set { this.SetValue(CellFontSizeProperty, value); }
        }

        public static readonly DependencyProperty CellFontSizeProperty = DependencyProperty.Register(
            "CellFontSize", typeof(int), typeof(FastGridControl), new PropertyMetadata(11, OnCellFontSizePropertyChanged));

        private static void OnCellFontSizePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl)dependencyObject).OnCellFontSizeChanged();
        }

        #endregion

        #region CellFontColor

        public Color CellFontColor
        {
            get { return (Color)this.GetValue(CellFontColorProperty); }
            set { this.SetValue(CellFontColorProperty, value); }
        }

        public static readonly DependencyProperty CellFontColorProperty = DependencyProperty.Register(
            "CellFontColor", typeof(Color), typeof(FastGridControl), new PropertyMetadata(Colors.Black, OnCellFontColorPropertyChanged));

        private static void OnCellFontColorPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl)dependencyObject).OnCellFontColorChanged();
        }

        #endregion

        #region HeaderBackground

        public Color HeaderBackground
        {
            get { return (Color)this.GetValue(HeaderBackgroundProperty); }
            set { this.SetValue(HeaderBackgroundProperty, value); }
        }

        public static readonly DependencyProperty HeaderBackgroundProperty = DependencyProperty.Register(
            "HeaderBackground", typeof(Color), typeof(FastGridControl), new PropertyMetadata(Color.FromRgb(0xF6, 0xF7, 0xF9), OnHeaderBackgroundPropertyChanged));

        private static void OnHeaderBackgroundPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl)dependencyObject).OnHeaderBackgroundChanged();
        }

        #endregion

        #region HeaderCurrentBackground

        public Color HeaderCurrentBackground
        {
            get { return (Color)this.GetValue(HeaderCurrentBackgroundProperty); }
            set { this.SetValue(HeaderCurrentBackgroundProperty, value); }
        }

        public static readonly DependencyProperty HeaderCurrentBackgroundProperty = DependencyProperty.Register(
            "HeaderCurrentBackground", typeof(Color), typeof(FastGridControl), new PropertyMetadata(Color.FromRgb(190, 207, 220), OnHeaderCurrentBackgroundPropertyChanged));

        private static void OnHeaderCurrentBackgroundPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl)dependencyObject).OnHeaderCurrentBackgroundChanged();
        }

        #endregion

        #region SelectedColor

        public Color SelectedColor
        {
            get { return (Color)this.GetValue(SelectedColorProperty); }
            set { this.SetValue(SelectedColorProperty, value); }
        }

        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
            "SelectedColor", typeof(Color), typeof(FastGridControl), new PropertyMetadata(Color.FromRgb(51, 153, 255), OnSelectedColorPropertyChanged));

        private static void OnSelectedColorPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl)dependencyObject).OnSelectedColorChanged();
        }

        #endregion

        #region SelectedTextColor

        public Color SelectedTextColor
        {
            get { return (Color)this.GetValue(SelectedTextColorProperty); }
            set { this.SetValue(SelectedTextColorProperty, value); }
        }

        public static readonly DependencyProperty SelectedTextColorProperty = DependencyProperty.Register(
            "SelectedTextColor", typeof(Color), typeof(FastGridControl), new PropertyMetadata(Colors.White, OnSelectedTextColorPropertyChanged));

        private static void OnSelectedTextColorPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl)dependencyObject).OnSelectedTextColorChanged();
        }

        #endregion

        #region LimitedSelectedColor

        public Color LimitedSelectedColor
        {
            get { return (Color)this.GetValue(LimitedSelectedColorProperty); }
            set { this.SetValue(LimitedSelectedColorProperty, value); }
        }

        public static readonly DependencyProperty LimitedSelectedColorProperty = DependencyProperty.Register(
            "LimitedSelectedColor", typeof(Color), typeof(FastGridControl), new PropertyMetadata(Color.FromRgb(51, 220, 220), OnLimitedSelectedColorPropertyChanged));

        private static void OnLimitedSelectedColorPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl)dependencyObject).OnLimitedSelectedColorChanged();
        }

        #endregion

        #region LimitedSelectedTextColor

        public Color LimitedSelectedTextColor
        {
            get { return (Color)this.GetValue(LimitedSelectedTextColorProperty); }
            set { this.SetValue(LimitedSelectedTextColorProperty, value); }
        }

        public static readonly DependencyProperty LimitedSelectedTextColorProperty = DependencyProperty.Register(
            "LimitedSelectedTextColor", typeof(Color), typeof(FastGridControl), new PropertyMetadata(Colors.White, OnLimitedSelectedTextColorPropertyChanged));

        private static void OnLimitedSelectedTextColorPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl)dependencyObject).OnLimitedSelectedTextColorChanged();
        }

        #endregion

        #region MouseOverRowColor

        public Color MouseOverRowColor
        {
            get { return (Color)this.GetValue(MouseOverRowColorProperty); }
            set { this.SetValue(MouseOverRowColorProperty, value); }
        }

        public static readonly DependencyProperty MouseOverRowColorProperty = DependencyProperty.Register(
            "MouseOverRowColor", typeof(Color), typeof(FastGridControl), new PropertyMetadata(Color.FromRgb(235, 235, 255), OnMouseOverRowColorPropertyChanged));

        private static void OnMouseOverRowColorPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl)dependencyObject).OnMouseOverRowColorChanged();
        }

        #endregion

        #region GridLineColor

        public Color GridLineColor
        {
            get { return (Color)this.GetValue(GridLineColorProperty); }
            set { this.SetValue(GridLineColorProperty, value); }
        }

        public static readonly DependencyProperty GridLineColorProperty = DependencyProperty.Register(
            "GridLineColor", typeof(Color), typeof(FastGridControl), new PropertyMetadata(Colors.LightGray, OnGridLineColorPropertyChanged));

        private static void OnGridLineColorPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl)dependencyObject).OnGridLineColorChanged();
        }

        #endregion

        #region ActiveRegionFrameColor

        public Color ActiveRegionFrameColor
        {
            get { return (Color)this.GetValue(ActiveRegionFrameColorProperty); }
            set { this.SetValue(ActiveRegionFrameColorProperty, value); }
        }

        public static readonly DependencyProperty ActiveRegionFrameColorProperty = DependencyProperty.Register(
            "ActiveRegionFrameColor", typeof(Color), typeof(FastGridControl), new PropertyMetadata(Color.FromRgb(0xAA, 0xAA, 0xFF), OnActiveRegionFrameColorPropertyChanged));

        private static void OnActiveRegionFrameColorPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl)dependencyObject).OnActiveRegionFrameColorChanged();
        }

        #endregion

        #region ActiveRegionHoverFillColor

        public Color ActiveRegionHoverFillColor
        {
            get { return (Color)this.GetValue(ActiveRegionHoverFillColorProperty); }
            set { this.SetValue(ActiveRegionHoverFillColorProperty, value); }
        }

        public static readonly DependencyProperty ActiveRegionHoverFillColorProperty = DependencyProperty.Register(
            "ActiveRegionHoverFillColor", typeof(Color), typeof(FastGridControl), new PropertyMetadata(Color.FromRgb(0xAA, 0xFF, 0xFF), OnActiveRegionHoverFillColorPropertyChanged));

        private static void OnActiveRegionHoverFillColorPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl)dependencyObject).OnActiveRegionHoverFillColorChanged();
        }

        #endregion

        #region CellPaddingHorizontal

        public int CellPaddingHorizontal
        {
            get { return (int)this.GetValue(CellPaddingHorizontalProperty); }
            set { this.SetValue(CellPaddingHorizontalProperty, value); }
        }

        public static readonly DependencyProperty CellPaddingHorizontalProperty = DependencyProperty.Register(
            "CellPaddingHorizontal", typeof(int), typeof(FastGridControl), new PropertyMetadata(2, OnCellPaddingHorizontalPropertyChanged));

        private static void OnCellPaddingHorizontalPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl)dependencyObject).OnCellPaddingHorizontalChanged();
        }

        #endregion

        #region CellPaddingVertical

        public int CellPaddingVertical
        {
            get { return (int)this.GetValue(CellPaddingVerticalProperty); }
            set { this.SetValue(CellPaddingVerticalProperty, value); }
        }

        public static readonly DependencyProperty CellPaddingVerticalProperty = DependencyProperty.Register(
            "CellPaddingVertical", typeof(int), typeof(FastGridControl), new PropertyMetadata(1, OnCellPaddingVerticalPropertyChanged));

        private static void OnCellPaddingVerticalPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl)dependencyObject).OnCellPaddingVerticalChanged();
        }

        #endregion

        #region BlockPadding

        public int BlockPadding
        {
            get { return (int)this.GetValue(BlockPaddingProperty); }
            set { this.SetValue(BlockPaddingProperty, value); }
        }

        public static readonly DependencyProperty BlockPaddingProperty = DependencyProperty.Register(
            "BlockPadding", typeof(int), typeof(FastGridControl), new PropertyMetadata(2, OnBlockPaddingPropertyChanged));

        private static void OnBlockPaddingPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl)dependencyObject).OnBlockPaddingChanged();
        }

        #endregion

        #region ColumnResizeTheresold

        public int ColumnResizeTheresold
        {
            get { return (int)this.GetValue(ColumnResizeTheresoldProperty); }
            set { this.SetValue(ColumnResizeTheresoldProperty, value); }
        }

        public static readonly DependencyProperty ColumnResizeTheresoldProperty = DependencyProperty.Register(
            "ColumnResizeTheresold", typeof(int), typeof(FastGridControl), new PropertyMetadata(2, OnColumnResizeTheresoldPropertyChanged));

        private static void OnColumnResizeTheresoldPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl)dependencyObject).OnColumnResizeTheresoldChanged();
        }

        #endregion

        #region MinColumnWidthOverride

        public int? MinColumnWidthOverride
        {
            get { return (int?)this.GetValue(MinColumnWidthOverrideProperty); }
            set { this.SetValue(MinColumnWidthOverrideProperty, value); }
        }

        public static readonly DependencyProperty MinColumnWidthOverrideProperty = DependencyProperty.Register(
            "MinColumnWidthOverride", typeof(int?), typeof(FastGridControl), new PropertyMetadata(null, OnMinColumnWidthOverridePropertyChanged));

        private static void OnMinColumnWidthOverridePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl)dependencyObject).OnMinColumnWidthOverrideChanged();
        }

        #endregion

        #region MaxRowHeaderWidth

        public int? MaxRowHeaderWidth
        {
            get { return (int?)this.GetValue(MaxRowHeaderWidthProperty); }
            set { this.SetValue(MaxRowHeaderWidthProperty, value); }
        }

        public static readonly DependencyProperty MaxRowHeaderWidthProperty = DependencyProperty.Register(
            "MaxRowHeaderWidth", typeof(int?), typeof(FastGridControl), new PropertyMetadata(null, OnMaxRowHeaderWidthPropertyChanged));

        private static void OnMaxRowHeaderWidthPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl)dependencyObject).OnMaxRowHeaderWidthChanged();
        }

        #endregion

        #region RowHeightReserve

        public int RowHeightReserve
        {
            get { return (int)this.GetValue(RowHeightReserveProperty); }
            set { this.SetValue(RowHeightReserveProperty, value); }
        }

        public static readonly DependencyProperty RowHeightReserveProperty = DependencyProperty.Register(
            "RowHeightReserve", typeof(int), typeof(FastGridControl), new PropertyMetadata(5, OnRowHeightReservePropertyChanged));

        private static void OnRowHeightReservePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl)dependencyObject).OnRowHeightReserveChanged();
        }

        #endregion

        #region WideColumnsLimit

        public int WideColumnsLimit
        {
            get { return (int)this.GetValue(WideColumnsLimitProperty); }
            set { this.SetValue(WideColumnsLimitProperty, value); }
        }

        public static readonly DependencyProperty WideColumnsLimitProperty = DependencyProperty.Register(
            "WideColumnsLimit", typeof(int), typeof(FastGridControl), new PropertyMetadata(250, OnWideColumnsLimitPropertyChanged));

        private static void OnWideColumnsLimitPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl)dependencyObject).OnWideColumnsLimitChanged();
        }

        #endregion

        private void OnAlternatingColorsChanged()
        {
            RenderChanged();
        }

        private void OnCellFontNameChanged()
        {
            RecalculateDefaultCellSize();
            RenderChanged();
        }

        private void OnCellFontColorChanged()
        {
            RenderGrid();
        }

        private void OnHeaderBackgroundChanged()
        {
            RenderChanged();
        }

        private void OnHeaderCurrentBackgroundChanged()
        {
            RenderChanged();
        }

        private void OnSelectedColorChanged()
        {
            RenderGrid();
        }

        private void OnSelectedTextColorChanged()
        {
            RenderGrid();
        }

        private void OnLimitedSelectedColorChanged()
        {
            RenderGrid();
        }

        private void OnLimitedSelectedTextColorChanged()
        {
            RenderGrid();
        }

        private void OnGridLineColorChanged()
        {
            RenderChanged();
        }

        private void OnCellPaddingHorizontalChanged()
        {
            RenderChanged();
        }

        private void OnCellPaddingVerticalChanged()
        {
            RenderChanged();
        }

        private void OnBlockPaddingChanged()
        {
            RenderChanged();
        }

        private void OnActiveRegionFrameColorChanged()
        {
            RenderChanged();
        }
        private void OnMinColumnWidthOverrideChanged()
        {
            RecalculateDefaultCellSize();
            InvalidateAll();
        }

        private void OnMaxRowHeaderWidthChanged()
        {
            RecalculateDefaultCellSize();
            InvalidateAll();
        }

        private void OnColumnResizeTheresoldChanged() { }

        public void OnMouseOverRowColorChanged() { }

        public int MinColumnWidth
        {
            get { return MinColumnWidthOverride ?? _columnSizes.DefaultSize; }
        }

        private void OnCellFontSizeChanged()
        {
            RecalculateDefaultCellSize();
            RenderChanged();
        }

        private void OnActiveRegionHoverFillColorChanged() { }

        private void OnRowHeightReserveChanged()
        {
            RecalculateDefaultCellSize();
            RenderGrid();
        }

        private void OnWideColumnsLimitChanged() { }
    }
}