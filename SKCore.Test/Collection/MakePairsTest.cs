using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SKCore.Collection;

namespace SKCore.Test.Collection
{
    [TestClass]
    public class MakePairsTest
    {
        [TestMethod]
        public void MakePairsTestNormal()
        {
            var source = new List<int> { 1, 2, 3, 4, 5, 6 };
            var result = source.MakePairs().ToList();

            Assert.IsTrue(result.SequenceEqual(new List<Tuple<int, int>>
            {
                Tuple.Create(1,2),
                Tuple.Create(2,3),
                Tuple.Create(3,4),
                Tuple.Create(4,5),
                Tuple.Create(5,6),
            }, new TupleEqualityCompare<int>()));
        }

        [TestMethod]
        public void MakePairsTestGivenEmpty()
        {
            var source = new List<int>();
            var result = source.MakePairs().ToList();

            Assert.IsTrue(result.SequenceEqual(new List<Tuple<int, int>>(), new TupleEqualityCompare<int>()));
        }

        [TestMethod]
        public void MakePairsTestGivenOddElementCount()
        {
            var source = new List<int> { 1, 2, 3, 4, 5 };
            var result = source.MakePairs().ToList();

            Assert.IsTrue(result.SequenceEqual(new List<Tuple<int, int>>
            {
                Tuple.Create(1,2),
                Tuple.Create(2,3),
                Tuple.Create(3,4),
                Tuple.Create(4,5),
            }, new TupleEqualityCompare<int>()));
        }
    }

    class TupleEqualityCompare<T> : IEqualityComparer<Tuple<T, T>>
    {
        public bool Equals(Tuple<T, T> x, Tuple<T, T> y)
        {
            return GetHashCode(x).Equals(GetHashCode(y));
        }

        public int GetHashCode(Tuple<T, T> obj)
        {
            return (obj.Item1?.GetHashCode() ?? GetHashCode()) ^ (obj.Item2?.GetHashCode() ?? GetHashCode());
        }
    }
}
