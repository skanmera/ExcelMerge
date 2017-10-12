using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SKCore.Collection;

namespace SKCore.Test.Collection
{
    [TestClass]
    public class TimesTest
    {
        [TestMethod]
        public void TimesTestGivenPositiveCount()
        {
            var result = new List<int>();

            10.Times(i => result.Add(i));

            Assert.IsTrue(Enumerable.Range(0, 10).ToList().SequenceEqual(result));
        }

        [TestMethod]
        public void TimesTestGivenPositiveStartNum()
        {
            var result = new List<int>();

            10.Times(5, i => result.Add(i));

            Assert.IsTrue(Enumerable.Range(5, 10).ToList().SequenceEqual(result));
        }

        [TestMethod]
        public void TimesTestGivenNegativeStartNum()
        {
            var result = new List<int>();

            10.Times(-10, i => result.Add(i));

            Assert.IsTrue(Enumerable.Range(-10, 10).ToList().SequenceEqual(result));
        }

        [TestMethod]
        public void TimesTestGivenNegativeCount()
        {
            var result = new List<int>();

            (-10).Times(i => result.Add(i));

            Assert.IsTrue(Enumerable.Range(-9, 10).Reverse().ToList().SequenceEqual(result));
        }

        [TestMethod]
        public void TimesTestGivenZero()
        {
            var result = new List<int>();

            (0).Times(i => result.Add(i));

            Assert.IsTrue(Enumerable.Empty<int>().SequenceEqual(result));
        }
    }
}
