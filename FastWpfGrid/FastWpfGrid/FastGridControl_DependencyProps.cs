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
    }
}