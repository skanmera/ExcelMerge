using System;
using System.Collections.Generic;
using FastWpfGrid;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FastWpfGridUnitTest
{
    [TestClass]
    public class GridTest
    {
        [TestMethod]
        public void TestSeriesHide()
        {
            var sizes = new SeriesSizes();
            sizes.Count = 10;
            for (int i = 0; i < sizes.Count; i++)
            {
                sizes.PutSizeOverride(i, i);
            }
            sizes.SetExtraordinaryIndexes(new HashSet<int> {2, 9}, new HashSet<int>());

            Assert.AreEqual(0, sizes.ModelToReal(0));
            Assert.AreEqual(1, sizes.ModelToReal(1));
            Assert.AreEqual(-1, sizes.ModelToReal(2));
            Assert.AreEqual(2, sizes.ModelToReal(3));
            Assert.AreEqual(7, sizes.ModelToReal(8));
            Assert.AreEqual(-1, sizes.ModelToReal(9));

            Assert.AreEqual(0, sizes.RealToModel(0));
            Assert.AreEqual(1, sizes.RealToModel(1));
            Assert.AreEqual(3, sizes.RealToModel(2));
            Assert.AreEqual(8, sizes.RealToModel(7));

            sizes.SetExtraordinaryIndexes(new HashSet<int>(), new HashSet<int> {3});
            Assert.AreEqual(1, sizes.ModelToReal(0));
            Assert.AreEqual(2, sizes.ModelToReal(1));
            Assert.AreEqual(3, sizes.ModelToReal(2));
            Assert.AreEqual(0, sizes.ModelToReal(3));
            Assert.AreEqual(4, sizes.ModelToReal(4));
        }
    }
}
