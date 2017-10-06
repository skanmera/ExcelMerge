using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SKCore.Collection;

namespace SKCore.Test.Collection
{
    [TestClass]
    public class EqualTest
    {
        [TestMethod]
        public void NestedSequenceEqualTest()
        {
            var seq1 = new List<List<int>>();
            seq1.Add(new List<int> { 0, 1, 2 });
            seq1.Add(new List<int> { 3, 4, 5 });
            seq1.Add(new List<int> { 6, 7, 8 });

            var seq2 = new List<List<int>>();
            seq2.Add(new List<int> { 0, 1, 2 });
            seq2.Add(new List<int> { 3, 4, 5 });
            seq2.Add(new List<int> { 6, 7, 8 });

            Assert.IsTrue(seq1.NestedSequenceEqual(seq2));
        }

        [TestMethod]
        public void NestedSequenceEqualTestWithEmpty()
        {
            var seq1 = new List<List<int>>();
            var seq2 = new List<List<int>>();

            Assert.IsTrue(seq1.NestedSequenceEqual(seq2));
        }

        [TestMethod]
        public void NestedSequenceEqualTestWithNotEqual()
        {
            var seq1 = new List<List<int>>();
            seq1.Add(new List<int> { 0, 1, 2 });
            seq1.Add(new List<int> { 3, 4, 5 });
            seq1.Add(new List<int> { 6, 7, 8 });

            var seq2 = new List<List<int>>();
            seq2.Add(new List<int> { 0, 1, 2 });
            seq2.Add(new List<int> { 3, 4, 5 });
            seq2.Add(new List<int> { 8, 9, 7 });

            Assert.IsFalse(seq1.SequenceEqual(seq2));
        }

        [TestMethod]
        public void NestedSequenceEqualTestWithReference()
        {
            var seq1 = new List<List<object>>();
            var obj = new object();
            seq1.Add(new List<object> { obj });
            seq1.Add(new List<object> { obj });
            seq1.Add(new List<object> { obj });

            var seq2 = new List<List<object>>();
            seq2.Add(new List<object> { obj });
            seq2.Add(new List<object> { obj });
            seq2.Add(new List<object> { obj });

            Assert.IsTrue(seq1.NestedSequenceEqual(seq2));
        }
    }
}
