using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SKCore.Collection;
using SKCore.Wpf.Controls.Utilities;

namespace SKCore.Wpf.Test.Controls.Utilities
{
    [TestClass]
    public class GridExtensionTest
    {
        [TestMethod]
        public void ClearChildrenTest()
        {
            var grid = new Grid();

            5.Times(() =>
            {
                var row = new RowDefinition { Height = new GridLength(100, GridUnitType.Pixel) };
                var col = new ColumnDefinition { Width = new GridLength(100, GridUnitType.Pixel) };

                grid.RowDefinitions.Add(row);
                grid.ColumnDefinitions.Add(col);
            });

            Assert.IsNull(grid.GetRow(new Point(0, -1)));
            Assert.AreEqual(grid.GetRow(new Point(0, 0)), 0);
            Assert.AreEqual(grid.GetRow(new Point(0, 1)), 0);
            Assert.AreEqual(grid.GetRow(new Point(0, 99)), 0);
            Assert.AreEqual(grid.GetRow(new Point(0, 100)), 1);
            Assert.AreEqual(grid.GetRow(new Point(0, 499)), 4);
            Assert.IsNull(grid.GetRow(new Point(0, 500)));

            Assert.IsNull(grid.GetColumn(new Point(-1, 0)));
            Assert.AreEqual(grid.GetColumn(new Point(0, 0)), 0);
            Assert.AreEqual(grid.GetColumn(new Point(1, 0)), 0);
            Assert.AreEqual(grid.GetColumn(new Point(99, 0)), 0);
            Assert.AreEqual(grid.GetColumn(new Point(100, 0)), 1);
            Assert.AreEqual(grid.GetColumn(new Point(499, 0)), 4);
            Assert.IsNull(grid.GetColumn(new Point(500, 0)));
        }
    }
}
