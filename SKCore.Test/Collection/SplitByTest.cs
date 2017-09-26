using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SKCore.Collection.Test
{
    [TestClass]
    public class SplitByTest
    {
        [TestMethod]
        public void SplitBySizeWith3()
        {
            var source = new List<int> { 1, 1, 1, 2, 2, 3, 3, 3, 3, 4, 4, 5, 6, 6 };
            var result = source.SplitBySize(3);

            Assert.IsTrue(result.NestedSequenceEqual(new List<List<int>>
            {
                new List<int> { 1, 1, 1 },
                new List<int> { 2, 2, 3 },
                new List<int> { 3, 3, 3 },
                new List<int> { 4, 4, 5 },
                new List<int> { 6, 6 },
            }));
        }

        [TestMethod]
        public void SplitBySizeWithEmpty()
        {
            var source = new List<int>();
            var result = source.SplitBySize(3);

            Assert.IsTrue(result.NestedSequenceEqual(new List<List<int>>()));
        }

        [TestMethod]
        public void SplitByEquality()
        {
            var source = new List<int> { 1, 1, 1, 2, 2, 3, 3, 3, 3, 4, 4, 5, 6, 6 };
            var result = source.SplitByEquality();

            Assert.IsTrue(result.NestedSequenceEqual(new List<List<int>>
            {
                new List<int> { 1, 1, 1 },
                new List<int> { 2, 2 },
                new List<int> { 3, 3, 3, 3 },
                new List<int> { 4, 4 },
                new List<int> { 5 },
                new List<int> { 6, 6 },
            }));
        }

        [TestMethod]
        public void SplitByEqualityWithMaxSize()
        {
            var source = new List<int> { 1, 1, 1, 2, 2, 3, 3, 3, 3, 4, 4, 5, 6, 6 };
            var result = source.SplitByEquality(3);

            Assert.IsTrue(result.NestedSequenceEqual(new List<List<int>>
            {
                new List<int> { 1, 1, 1 },
                new List<int> { 2, 2 },
                new List<int> { 3, 3, 3 },
                new List<int> { 3 },
                new List<int> { 4, 4 },
                new List<int> { 5 },
                new List<int> { 6, 6 },
            }));
        }

        [TestMethod]
        public void SplitByEqualityWithEmpty()
        {
            var source = new List<int>();
            var result = source.SplitByEquality();

            Assert.IsTrue(result.NestedSequenceEqual(new List<List<int>>()));
        }

        [TestMethod]
        public void SplitByRegulalityConditionIncrease()
        {
            var source = new List<int> { 1, 1, 1, 2, 2, 3, 3, 3, 3, 4, 4, 5, 6, 6, 1, 1, 2, 3, 3, 3, 4 };
            var result = source.SplitByRegularity((p, c, gi, li) => p <= c);

            Assert.IsTrue(result.NestedSequenceEqual(new List<List<int>>
            {
                new List<int>{ 1, 1, 1, 2, 2, 3, 3, 3, 3, 4, 4, 5, 6, 6,},
                new List<int>{ 1, 1, 2, 3, 3, 3, 4 },
            }));
        }

        [TestMethod]
        public void SplitByRegulalityConditionSerialNumber()
        {
            var source = new List<int> { 1, 2, 3, 5, 6, 5, 6, 7, 8, -2, -1, 0, 2 };
            var result = source.SplitByRegularity((p, c, gi, li) => c == p + 1);

            Assert.IsTrue(result.NestedSequenceEqual(new List<List<int>>
            {
                new List<int> { 1, 2, 3 },
                new List<int> { 5, 6 },
                new List<int> { 5, 6, 7, 8 },
                new List<int> { -2, -1, 0 },
                new List<int> { 2 },
            }));
        }

        [TestMethod]
        public void SplitByRegulalityConditionLessOrEqualGlobalIndex()
        {
                        /* GlobalIndex   0  1  2  3  4   5  6  7  8  9  10  11  12  13  14  15  16  17  18  */
            var source = new List<int> { 0, 0, 1, 1, 5, -1, 6, 0, 9, 2,  1,  3,  4, 20,  1,  0, 30, 18,  0,};
            var result = source.SplitByRegularity((p, c, gi, li) => c <= gi);

            Assert.IsTrue(result.NestedSequenceEqual(new List<List<int>>
            {
                new List<int> { 0, 0, 1, 1 },
                new List<int> { 5, -1, 6, 0 },
                new List<int> { 9, 2, 1, 3, 4 },
                new List<int> { 20, 1, 0 },
                new List<int> { 30 },
                new List<int> { 18, 0 },
            }));
        }
    }
}
