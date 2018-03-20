using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace NetDiff.Test
{
    /// <summary>
    /// Naming rules: (Description of given data)_(Order and Option)_(Expected behavior)
    /// </summary>
    [TestClass]
    public class Test
    {
        [TestMethod]
        private void EqualSeq_None_ResultIsCorrect(DiffOrderType orderType)
        {
            var str1 = "abcd";
            var str2 = "abcd";

            var diff = DiffUtil.OptimizedDiff(str1, str2);
            Assert.AreEqual(str1.Count(), diff.Count());
            Assert.IsTrue(diff.All(r => r.Status == DiffStatus.Equal));
            Assert.IsTrue(diff.Select(r => r.Obj1).SequenceEqual(str1.Select(c => c)));
            Assert.IsTrue(diff.Select(r => r.Obj2).SequenceEqual(str1.Select(c => c)));
        }

        [TestMethod]
        public void RondomString_None_EqualToRestoredString()
        {
            var str1 = "q4DU8sbeD4JdhFA4hWShCv3bbtD7djX5SaNnQUHJHdCEJs6X2LJipbEEr7bZZbzcUrpuKpRDKNz92x5P";
            var str2 = "3GKLWNDdCxip8kda2r2MUT45RrHUiESQhmhUZtMcpBGcSwJVS9uq4DWBAQk2zPUJCJabaeWuP5mxyPBz";

            var diff = DiffUtil.OptimizedDiff(str1, str2);
            var src = new string(diff.CreateSrc().ToArray());
            var dst = new string(diff.CreateDst().ToArray());
            Assert.AreEqual(src, str1);
            Assert.AreEqual(dst, str2);
        }

        [TestMethod]
        public void CompletelyDifferent_None_ResultIsCorrect()
        {
            var str1 = "aaa";
            var str2 = "bbb";

            var diff = DiffUtil.OptimizedDiff(str1, str2);
            Assert.AreEqual(DiffStatus.Modified, diff.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Modified, diff.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Modified, diff.ElementAt(2).Status);
        }

        [TestMethod]
        public void Appended_None_ResultIsCorrect()
        {
            var str1 = "abc";
            var str2 = "abcd";

            var diff = DiffUtil.Diff(str1, str2);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Inserted, diff.ElementAt(3).Status);
        }

        [TestMethod]
        public void Prepended_None_ResultIsCorrect()
        {
            var str1 = "abc";
            var str2 = "_abc";

            var diff = DiffUtil.OptimizedDiff(str1, str2);
            Assert.AreEqual(DiffStatus.Inserted, diff.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(3).Status);
        }

        [TestMethod]
        public void Inserted_None_ResultIsCorrect()
        {
            var str1 = "abcd";
            var str2 = "ab_cd";

            var diff = DiffUtil.OptimizedDiff(str1, str2);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Inserted, diff.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(4).Status);
        }

        [TestMethod]
        public void Popped_None_ResultIsCorrect()
        {
            var str1 = "abc_";
            var str2 = "abc";

            var diff = DiffUtil.OptimizedDiff(str1, str2);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Deleted, diff.ElementAt(3).Status);
        }

        [TestMethod]
        public void Deleted_None_ResultIsCorrect()
        {
            var str1 = "ab_cd";
            var str2 = "abcd";

            var diff = DiffUtil.OptimizedDiff(str1, str2);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Deleted, diff.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(4).Status);
        }

        [TestMethod]
        public void Shifted_None_ResultIsCorrect()
        {
            var str1 = "_abc";
            var str2 = "abc";

            var diff = DiffUtil.OptimizedDiff(str1, str2);

            Assert.AreEqual(DiffStatus.Deleted, diff.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(3).Status);
        }

        [TestMethod]
        public void ModifiedAlternately_None_ResultIsCorrect()
        {
            var str1 = "abcdefg";
            var str2 = "a_c_e_g";

            var diff = DiffUtil.OptimizedDiff(str1, str2);

            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Modified, diff.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Modified, diff.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(4).Status);
            Assert.AreEqual(DiffStatus.Modified, diff.ElementAt(5).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(6).Status);
        }

        [TestMethod]
        public void ModifiedEquivalentSeriesFirst_None_ResultIsCorrect()
        {
            string str1 = "abbbc";
            string str2 = "a_bbc";

            var diff = DiffUtil.OptimizedDiff(str1, str2);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Modified, diff.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(4).Status);
        }

        [TestMethod]
        public void ModifiedEquivalentSeriesLast_None_ResultIsCorrect()
        {
            string str1 = "abbbc";
            string str2 = "abb_c";

            var diff = DiffUtil.OptimizedDiff(str1, str2);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Modified, diff.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(4).Status);
        }

        [TestMethod]
        public void ModifiedEquivalentSeriesCenter_None_ResultIsCorrect()
        {
            string str1 = "abbbc";
            string str2 = "ab_bc";

            var diff = DiffUtil.OptimizedDiff(str1, str2);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Modified, diff.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(4).Status);
        }

        [TestMethod]
        public void ModifiedSeries_None_ResultIsCorrect()
        {
            string str1 = "abbbc";
            string str2 = "a___c";

            var diff = DiffUtil.OptimizedDiff(str1, str2);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Modified, diff.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Modified, diff.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Modified, diff.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(4).Status);
        }

        [TestMethod]
        public void DeletedNextToModified_None_ResultIsCorrect()
        {
            string str1 = "abcdef";
            string str2 = "ab_ef";

            var diff = DiffUtil.OptimizedDiff(str1, str2);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Deleted, diff.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Modified, diff.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(4).Status);
        }

        [TestMethod]
        public void InsertedNextToModified_None_ResultIsCorrect()
        {
            string str1 = "abcdef";
            string str2 = "ab_gdef";

            var diff = DiffUtil.OptimizedDiff(str1, str2);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Inserted, diff.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Modified, diff.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(4).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(5).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(6).Status);
        }

        [TestMethod]
        public void ModifiedBetweenInsertedAndInserted_None_ResultIsCorrect()
        {
            string str1 = "abcde";
            string str2 = "ab_g_de";

            var diff = DiffUtil.OptimizedDiff(str1, str2);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Inserted, diff.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Inserted, diff.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Modified, diff.ElementAt(4).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(5).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(6).Status);
        }

        [TestMethod]
        public void AppendAndShifted_None_ResultIsCorrect()
        {
            var str1 = "_aaa";
            var str2 = "aaa_";

            var diff = DiffUtil.OptimizedDiff(str1, str2);
            Assert.AreEqual(DiffStatus.Deleted, diff.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Inserted, diff.ElementAt(4).Status);
        }

        [TestMethod]
        public void DstContainsUppercase_IgnoreCaseCompare_ResultIsCorrect()
        {
            var str1 = "abc";
            var str2 = "dBf";

            var option = new DiffOption<char>
            {
                EqualityComparer = new IgnoreCaseCompare()
            };

            var opt = DiffUtil.OptimizedDiff(str1, str2, option);
            Assert.AreEqual(DiffStatus.Modified, opt.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Equal, opt.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Modified, opt.ElementAt(2).Status);
        }

        [TestMethod]
        public void BothAreEmpty_ReturenEmpty()
        {
            string str1 = string.Empty;
            string str2 = string.Empty;

            var diff = DiffUtil.OptimizedDiff(str1, str2);

            Assert.IsTrue(!diff.Any());
        }

        [TestMethod]
        public void SrcIsEmpty_None_NoException()
        {
            string str1 = "";
            string str2 = "abcde";

            var diff = DiffUtil.OptimizedDiff(str1, str2);

            Assert.AreEqual(DiffStatus.Inserted, diff.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Inserted, diff.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Inserted, diff.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Inserted, diff.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Inserted, diff.ElementAt(4).Status);
        }

        [TestMethod]
        public void DstIsEmpty_None_NoException()
        {
            string str1 = "abced";
            string str2 = "";

            var diff = DiffUtil.OptimizedDiff(str1, str2);

            Assert.AreEqual(DiffStatus.Deleted, diff.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Deleted, diff.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Deleted, diff.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Deleted, diff.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Deleted, diff.ElementAt(4).Status);
        }

        [TestMethod]
        public void ModifiedCenter_GreedyDeleteFirst_OrderIsCollect()
        {
            string str1 = "abc";
            string str2 = "a_c";

            var diff = DiffUtil.Diff(str1, str2).Order(DiffOrderType.GreedyDeleteFirst);

            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Deleted, diff.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Inserted, diff.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(3).Status);
        }

        [TestMethod]
        public void ModifiedCenter_GreedyInsertFirst_OrderIsCollect()
        {
            string str1 = "abc";
            string str2 = "a_c";

            var diff = DiffUtil.Diff(str1, str2).Order(DiffOrderType.GreedyInsertFirst);

            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Inserted, diff.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Deleted, diff.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(3).Status);
        }

        [TestMethod]
        public void ModifiedCenter_LazyDeleteFirst_OrderIsCollect()
        {
            string str1 = "abc";
            string str2 = "a_c";

            var diff = DiffUtil.Diff(str1, str2).Order(DiffOrderType.LazyDeleteFirst);

            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Deleted, diff.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Inserted, diff.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(3).Status);
        }

        [TestMethod]
        public void ModifiedCenter_LazyInsertFirst_OrderIsCollect()
        {
            string str1 = "abc";
            string str2 = "a_c";

            var diff = DiffUtil.Diff(str1, str2).Order(DiffOrderType.LazyInsertFirst);

            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Inserted, diff.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Deleted, diff.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(3).Status);
        }

        [TestMethod]
        public void ModifiedSeries_GreedyDeleteFirst_OrderIsCollect()
        {
            string str1 = "abbc";
            string str2 = "a__c";

            var diff = DiffUtil.Diff(str1, str2).Order(DiffOrderType.GreedyDeleteFirst);

            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Deleted, diff.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Deleted, diff.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Inserted, diff.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Inserted, diff.ElementAt(4).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(5).Status);
        }

        [TestMethod]
        public void ModifiedSeries_GreedyInsertFirst_OrderIsCollect()
        {
            string str1 = "abbc";
            string str2 = "a__c";

            var diff = DiffUtil.Diff(str1, str2).Order(DiffOrderType.GreedyInsertFirst);

            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Inserted, diff.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Inserted, diff.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Deleted, diff.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Deleted, diff.ElementAt(4).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(5).Status);
        }

        [TestMethod]
        public void ModifiedSeries_LazyDeleteFirst_OrderIsCollect()
        {
            string str1 = "abbc";
            string str2 = "a__c";

            var diff = DiffUtil.Diff(str1, str2).Order(DiffOrderType.LazyDeleteFirst);

            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Deleted, diff.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Inserted, diff.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Deleted, diff.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Inserted, diff.ElementAt(4).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(5).Status);
        }

        [TestMethod]
        public void ModifiedSeries_LazyInsertFirst_OrderIsCollect()
        {
            string str1 = "abbc";
            string str2 = "a__c";

            var diff = DiffUtil.Diff(str1, str2).Order(DiffOrderType.LazyInsertFirst);

            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Inserted, diff.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Deleted, diff.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Inserted, diff.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Deleted, diff.ElementAt(4).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(5).Status);
        }

        [TestMethod]
        public void SwapedEquivalentSeries_GreedyInsertFirst_OrderIsCollect()
        {
            string str1 = "aaa_bbb";
            string str2 = "bbb_aaa";

            var diff = DiffUtil.Diff(str1, str2).Order(DiffOrderType.LazyInsertFirst);

            Assert.AreEqual(DiffStatus.Deleted, diff.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Deleted, diff.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Deleted, diff.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Deleted, diff.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(4).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(5).Status);
            Assert.AreEqual(DiffStatus.Equal, diff.ElementAt(6).Status);
            Assert.AreEqual(DiffStatus.Inserted, diff.ElementAt(7).Status);
            Assert.AreEqual(DiffStatus.Inserted, diff.ElementAt(8).Status);
            Assert.AreEqual(DiffStatus.Inserted, diff.ElementAt(9).Status);
            Assert.AreEqual(DiffStatus.Inserted, diff.ElementAt(10).Status);
        }

        [TestMethod]
        public void BothAreNull_None_ReturenEmpty()
        {
            string str1 = null;
            string str2 = null;

            var diff = DiffUtil.OptimizedDiff(str1, str2);

            Assert.IsTrue(!diff.Any());
        }

        [TestMethod]
        public void SrcIsNull_None_ReturenEmpty()
        {
            string str1 = null;
            var str2 = string.Empty;

            var diff = DiffUtil.OptimizedDiff(str1, str2);

            Assert.IsTrue(!diff.Any());
        }

        [TestMethod]
        public void DstIsNull_None_ReturenEmpty()
        {
            string str1 = null;
            var str2 = string.Empty;

            var diff = DiffUtil.OptimizedDiff(str1, str2);

            Assert.IsTrue(!diff.Any());
        }

        [TestMethod]
        public void None_None_LimitOptionWorks()
        {
            var str1 = Enumerable.Repeat("Good dog", 1000).SelectMany(c => c);
            var str2 = Enumerable.Repeat("Bad dog", 1000).SelectMany(c => c);

            var option = new DiffOption<char>
            {
                Limit = 1000
            };

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            DiffUtil.OptimizedDiff(str1, str2);
            sw.Stop();
            var time1 = sw.Elapsed;

            sw.Restart();
            DiffUtil.OptimizedDiff(str1, str2, option);
            sw.Stop();
            var time2 = sw.Elapsed;

            Assert.IsTrue(time2 < time1);
        }

        internal class IgnoreCaseCompare : IEqualityComparer<char>
        {
            public bool Equals(char x, char y)
            {
                return x.ToString().ToLower().Equals(y.ToString().ToLower());
            }

            public int GetHashCode(char obj)
            {
                return obj.ToString().ToLower().GetHashCode();
            }
        }
    }
}
