using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NetDiff.Test
{
    [TestClass]
    public class Test
    {
        [TestMethod]
        public void Equal_GreedyDeleteFirst()
        {
            var str1 = "abcde";
            var str2 = "abcde";

            var results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.GreedyDeleteFirst);

            Assert.AreEqual(str1.Count(), results.Count());
            Assert.IsTrue(results.All(r => r.Status == DiffStatus.Equal));
            Assert.IsTrue(results.Select(r => r.Obj1).SequenceEqual(str1.Select(c => c)));
            Assert.IsTrue(results.Select(r => r.Obj2).SequenceEqual(str1.Select(c => c)));
        }

        [TestMethod]
        public void Equal_GreedyInsertFirst()
        {
            var str1 = "abcde";
            var str2 = "abcde";

            var results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.GreedyInsertFirst);

            Assert.AreEqual(str1.Count(), results.Count());
            Assert.IsTrue(results.All(r => r.Status == DiffStatus.Equal));
            Assert.IsTrue(results.Select(r => r.Obj1).SequenceEqual(str1.Select(c => c)));
            Assert.IsTrue(results.Select(r => r.Obj2).SequenceEqual(str1.Select(c => c)));
        }

        [TestMethod]
        public void Equal_LazyDeleteFirst()
        {
            var str1 = "abcde";
            var str2 = "abcde";

            var results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.LazyDeleteFirst);

            Assert.AreEqual(str1.Count(), results.Count());
            Assert.IsTrue(results.All(r => r.Status == DiffStatus.Equal));
            Assert.IsTrue(results.Select(r => r.Obj1).SequenceEqual(str1.Select(c => c)));
            Assert.IsTrue(results.Select(r => r.Obj2).SequenceEqual(str1.Select(c => c)));
        }

        [TestMethod]
        public void Equal_LazyInsertFirst()
        {
            var str1 = "abcde";
            var str2 = "abcde";

            var results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.LazyInsertFirst);

            Assert.AreEqual(str1.Count(), results.Count());
            Assert.IsTrue(results.All(r => r.Status == DiffStatus.Equal));
            Assert.IsTrue(results.Select(r => r.Obj1).SequenceEqual(str1.Select(c => c)));
            Assert.IsTrue(results.Select(r => r.Obj2).SequenceEqual(str1.Select(c => c)));
        }

        [TestMethod]
        public void Restore_GreedyDeleteFirst()
        {
            var str1 = "q4DU8sbeD4JdhFA4hWShCv3bbtD7djX5SaNnQUHJHdCEJs6X2LJipbEEr7bZZbzcUrpuKpRDKNz92x5P";
            var str2 = "3GKLWNDdCxip8kda2r2MUT45RrHUiESQhmhUZtMcpBGcSwJVS9uq4DWBAQk2zPUJCJabaeWuP5mxyPBz";

            var results = DiffUtil.Diff(str1, str2 );
            results = DiffUtil.Order(results, DiffOrderType.GreedyDeleteFirst);

            var src = new string(DiffUtil.CreateSrc(results).ToArray());
            var dst = new string(DiffUtil.CreateDst(results).ToArray());
            Assert.AreEqual(dst, str2);
            Assert.AreEqual(src, str1);
        }

        [TestMethod]
        public void Restore_GreedyInsertFirst()
        {
            var str1 = "q4DU8sbeD4JdhFA4hWShCv3bbtD7djX5SaNnQUHJHdCEJs6X2LJipbEEr7bZZbzcUrpuKpRDKNz92x5P";
            var str2 = "3GKLWNDdCxip8kda2r2MUT45RrHUiESQhmhUZtMcpBGcSwJVS9uq4DWBAQk2zPUJCJabaeWuP5mxyPBz";

            var results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.GreedyInsertFirst);

            var src = new string(DiffUtil.CreateSrc(results).ToArray());
            var dst = new string(DiffUtil.CreateDst(results).ToArray());
            Assert.AreEqual(dst, str2);
            Assert.AreEqual(src, str1);
        }

        [TestMethod]
        public void Restore_LazyInsertFirst()
        {
            var str1 = "q4DU8sbeD4JdhFA4hWShCv3bbtD7djX5SaNnQUHJHdCEJs6X2LJipbEEr7bZZbzcUrpuKpRDKNz92x5P";
            var str2 = "3GKLWNDdCxip8kda2r2MUT45RrHUiESQhmhUZtMcpBGcSwJVS9uq4DWBAQk2zPUJCJabaeWuP5mxyPBz";

            var results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.LazyInsertFirst);

            var src = new string(DiffUtil.CreateSrc(results).ToArray());
            var dst = new string(DiffUtil.CreateDst(results).ToArray());
            Assert.AreEqual(dst, str2);
            Assert.AreEqual(src, str1);
        }

        [TestMethod]
        public void Restore_LazyDeleteFirst()
        {
            var str1 = "q4DU8sbeD4JdhFA4hWShCv3bbtD7djX5SaNnQUHJHdCEJs6X2LJipbEEr7bZZbzcUrpuKpRDKNz92x5P";
            var str2 = "3GKLWNDdCxip8kda2r2MUT45RrHUiESQhmhUZtMcpBGcSwJVS9uq4DWBAQk2zPUJCJabaeWuP5mxyPBz";

            var results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.LazyDeleteFirst);

            var src = new string(DiffUtil.CreateSrc(results).ToArray());
            var dst = new string(DiffUtil.CreateDst(results).ToArray());
            Assert.AreEqual(dst, str2);
            Assert.AreEqual(src, str1);
        }

        /*
             a a a b b b 
             - - - + + +
        */
        [TestMethod]
        public void DifferentAll_GreedyDeleteFirst()
        {
            var str1 = "aaa";
            var str2 = "bbb";

            var results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.GreedyDeleteFirst);

            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(4).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(5).Status);
        }

        /*
             b b b a a a 
             + + + - - -
        */
        [TestMethod]
        public void DifferentAll_GreedyInsertFirst()
        {
            var str1 = "aaa";
            var str2 = "bbb";

            var results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.GreedyInsertFirst);

            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(4).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(5).Status);
        }

        /*
             a b a b a b
             - + - + - +
        */
        [TestMethod]
        public void DifferentAll_LazyDeleteFirst()
        {
            var str1 = "aaa";
            var str2 = "bbb";

            var results = DiffUtil.Diff(str1, str2 );
            results = DiffUtil.Order(results, DiffOrderType.LazyDeleteFirst).ToList();

            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(4).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(5).Status);
        }

        /*
             b a b a b a
             + - + - + -
        */
        [TestMethod]
        public void  DifferentAll_LazyInsertFirst()
        {
            var str1 = "aaa";
            var str2 = "bbb";

            var results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.LazyInsertFirst);

            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(4).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(5).Status);
        }

        /*
             a b c d
             = = = +
        */
        [TestMethod]
        public void  Appended()
        {
            var str1 = "abc";
            var str2 = "abcd";

            var results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.GreedyDeleteFirst);

            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(3).Status);

            results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.GreedyInsertFirst);

            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(3).Status);

            results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.LazyDeleteFirst);

            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(3).Status);

            results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.LazyInsertFirst);

            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(3).Status);
        }

        /*
             a b c d
             + = = = 
        */
        [TestMethod]
        public void Prepended()
        {
            var str1 = "bcd";
            var str2 = "abcd";

            var results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.GreedyDeleteFirst);

            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(3).Status);

            results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.GreedyInsertFirst);

            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(3).Status);

            results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.LazyDeleteFirst);

            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(3).Status);

            results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.LazyInsertFirst);

            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(3).Status);
        }

        [TestMethod]
        public void CaseMultiSameScore_GreedyDeleteFirst()
        {
            var str1 = "cdhijkz";
            var str2 = "ldxhnokz";

            var results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.GreedyDeleteFirst);

            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(4).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(5).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(6).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(7).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(8).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(9).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(10).Status);
        }

        [TestMethod]
        public void CaseMultiSameScore_GreedyInsertFirst()
        {
            var str1 = "cdhijkz";
            var str2 = "ldxhnokz";

            var results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.GreedyInsertFirst);

            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(4).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(5).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(6).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(7).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(8).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(9).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(10).Status);
        }

        [TestMethod]
        public void CaseMultiSameScore_LazyDeleteFirst()
        {
            var str1 = "cdhijkz";
            var str2 = "ldxhnokz";

            var results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.LazyDeleteFirst);

            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(4).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(5).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(6).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(7).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(8).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(9).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(10).Status);
        }

        [TestMethod]
        public void CaseMultiSameScore_LazyInsertFirst()
        {
            var str1 = "cdhijkz";
            var str2 = "ldxhnokz";

            var results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.LazyInsertFirst);

            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(4).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(5).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(6).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(7).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(8).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(9).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(10).Status);
        }

        [TestMethod]
        public void OptimizeCaseDeleteFirst()
        {
            var str1 = "aaa";
            var str2 = "bbb";

            /*
                obj1  a a a 
                obj2      b b b  
                      - - M + + 
            */
            var results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.GreedyDeleteFirst);
            results = DiffUtil.OptimizeCaseDeletedFirst(results);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Modified, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(4).Status);


            /*
                obj1      a a a 
                obj2  b b b
                      + + M - -
            */
            results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.GreedyInsertFirst);
            results = DiffUtil.OptimizeCaseInsertedFirst(results);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Modified, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(4).Status);


            /*
                obj1  a a a
                obj2  b b b   
                      M M M
            */
            results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.LazyDeleteFirst);
            results = DiffUtil.OptimizeCaseDeletedFirst(results);
            Assert.AreEqual(DiffStatus.Modified, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Modified, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Modified, results.ElementAt(2).Status);

            /*
                obj1  a a a
                obj2  b b b   
                      M M M
            */
            results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.LazyInsertFirst);
            results = DiffUtil.OptimizeCaseInsertedFirst(results);
            Assert.AreEqual(DiffStatus.Modified, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Modified, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Modified, results.ElementAt(2).Status);
        }

        [TestMethod]
        public void CaseRepeat_GreedyInsertFirst()
        {
            string str1 = "abbbc";
            string str2 = "adbbc";

            var results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.GreedyInsertFirst);

            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(4).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(5).Status);
        }

        [TestMethod]
        public void CaseRepeat_GreedyDeleteFirst()
        {
            string str1 = "abbbc";
            string str2 = "adbbc";

            var results = DiffUtil.Diff(str1, str2 );
            results = DiffUtil.Order(results, DiffOrderType.GreedyDeleteFirst);

            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(4).Status);
        }

        [TestMethod]
        public void CaseRepeat_LazyInsertFirst()
        {
            string str1 = "abbbc";
            string str2 = "adbbc";

            var results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.LazyInsertFirst);

            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(4).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(5).Status);
        }

        [TestMethod]
        public void CaseRepeat_LazyDeleteFirst()
        {
            string str1 = "abbbc";
            string str2 = "adbbc";

            var results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.LazyDeleteFirst);

            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(4).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(5).Status);
        }


        [TestMethod]
        public void SpecifiedComparer()
        {
            var str1 = "abc";
            var str2 = "dBf";

            var option = new DiffOption<char>();
            option.EqualityComparer = new CaseInsensitiveComparer();

            var results = DiffUtil.Diff(str1, str2, option);
            results = DiffUtil.Order(results, DiffOrderType.LazyDeleteFirst);

            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(4).Status);
        }

        [TestMethod]
        public void CaseReplace()
        {
            string str1 = "abbbc";
            string str2 = "adbbc";

            var results = DiffUtil.Diff(str1, str2);
            results = DiffUtil.Order(results, DiffOrderType.GreedyDeleteFirst);

            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(4).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(5).Status);
        }

        [TestMethod]
        public void GivenEmpty()
        {
            string str1 = "";
            string str2 = "";

            var results = DiffUtil.Diff(str1, str2);

            Assert.IsTrue(!results.Any());
        }

        [TestMethod]
        public void GivenSeq1Empty()
        {
            string str1 = "";
            string str2 = "abcde";

            var results = DiffUtil.Diff(str1, str2);

            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(4).Status);
        }

        [TestMethod]
        public void GivenSeq2Empty()
        {
            string str1 = "abced";
            string str2 = "";

            var results = DiffUtil.Diff(str1, str2);

            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(4).Status);
        }

        [TestMethod]
        public void Performance()
        {
            var str1 = Enumerable.Repeat("Good dog", 1000).SelectMany(c => c);
            var str2 = Enumerable.Repeat("Bad dog", 1000).SelectMany(c => c);

            var option = new DiffOption<char>();
            option.Limit = 1000;

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var result1 = DiffUtil.Diff(str1, str2);
            sw.Stop();
            var time1 = sw.Elapsed;

            sw.Restart();
            var result2 = DiffUtil.Diff(str1, str2, option);
            sw.Stop();
            var time2 = sw.Elapsed;

            Assert.IsTrue(time2 < time1);
        }

        [TestMethod]
        public void CaseEmpty()
        {
            var str1 = string.Empty;
            var str2 = string.Empty;

            var results = DiffUtil.Diff(str1, str2);

            Assert.IsTrue(!results.Any());
        }

        [TestMethod]
        public void CaseNull()
        {
            string str1 = null;
            var str2 = string.Empty;

            var results = DiffUtil.Diff(str1, str2);

            Assert.IsTrue(!results.Any());
        }

        internal class CaseInsensitiveComparer : IEqualityComparer<char>
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
