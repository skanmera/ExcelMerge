using System;
using System.Collections.Generic;
using System.Linq;

namespace NetDiff
{
    public class DiffUtil
    {
        public static IEnumerable<DiffResult<T>> Diff<T>(IEnumerable<T> seq1, IEnumerable<T> seq2)
        {
            return Diff(seq1, seq2, new DiffOption<T>());
        }

        public static IEnumerable<DiffResult<T>> Diff<T>(IEnumerable<T> seq1, IEnumerable<T> seq2, DiffOption<T> option)
        {
            if (seq1 == null || seq2 == null || (!seq1.Any() && !seq2.Any()))
                return Enumerable.Empty<DiffResult<T>>();

            var editGrap = new EditGraph<T>(seq1, seq2);
            var waypoints = editGrap.CalculatePath(option);

            return MakeResults<T>(waypoints, seq1, seq2);
        }

        public static IEnumerable<DiffResult<T>> OptimizedDiff<T>(IEnumerable<T> seq1, IEnumerable<T> seq2)
        {
            return OptimizedDiff(seq1, seq2, new DiffOption<T>());
        }

        public static IEnumerable<DiffResult<T>> OptimizedDiff<T>(IEnumerable<T> seq1, IEnumerable<T> seq2, DiffOption<T> option)
        {
            return Diff(seq1, seq2, option).Optimize(option.EqualityComparer);
        }

        public static IEnumerable<T> CreateSrc<T>(IEnumerable<DiffResult<T>> diffResults)
        {
            return diffResults.Where(r => r.Status != DiffStatus.Inserted).Select(r => r.Obj1);
        }

        public static IEnumerable<T> CreateDst<T>(IEnumerable<DiffResult<T>> diffResults)
        {
            return diffResults.Where(r => r.Status != DiffStatus.Deleted).Select(r => r.Obj2);
        }

        public static IEnumerable<DiffResult<T>> Optimaize<T>(IEnumerable<DiffResult<T>> diffResults, IEqualityComparer<T> compare = null)
        {
            var srcArray = new NullableDiffObject<T>[diffResults.Count()];
            var dstArray = new NullableDiffObject<T>[srcArray.Length];

            var count = 0;
            foreach (var result in diffResults)
            {
                switch (result.Status)
                {
                    case DiffStatus.Deleted:
                        srcArray[count] = new NullableDiffObject<T>(result.Obj1);
                        break;
                    case DiffStatus.Equal:
                        srcArray[count] = new NullableDiffObject<T>(result.Obj1);
                        dstArray[count] = new NullableDiffObject<T>(result.Obj2);
                        break;
                    case DiffStatus.Inserted:
                        dstArray[count] = new NullableDiffObject<T>(result.Obj2);
                        break;
                }

                count++;
            }

            var compactedCount = Compact(ref srcArray, ref dstArray);

            for (int i = 0, max = srcArray.Length; i < max; i++)
            {
                if (dstArray[i] == null)
                {
                    yield return new DiffResult<T>(srcArray[i].Obj, default(T), DiffStatus.Deleted);
                }
                else if (srcArray[i] == null)
                {
                    yield return new DiffResult<T>(default(T), dstArray[i].Obj, DiffStatus.Inserted);
                }
                else if (Equals(srcArray[i].Obj, dstArray[i].Obj, compare))
                {
                    yield return new DiffResult<T>(srcArray[i].Obj, dstArray[i].Obj, DiffStatus.Equal);
                }
                else
                {
                    yield return new DiffResult<T>(srcArray[i].Obj, dstArray[i].Obj, DiffStatus.Modified);
                }
            }
        }

        private static int Compact<T>(ref NullableDiffObject<T>[] srcArray, ref NullableDiffObject<T>[] dstArray, IEqualityComparer<T> compare = null)
        {
            for (int i = 0, max = srcArray.Length * 2; i < max; i++)
            {
                var isTargetSrc = i % 2 == 0;
                var srcIndex = i / 2;

                var targetArray = isTargetSrc ? srcArray : dstArray;
                var oppositeArray = isTargetSrc ? dstArray : srcArray;

                if (targetArray[srcIndex] != null && oppositeArray[srcIndex] == null)
                {
                    var currentIndex = srcIndex;
                    var dstIndex = -1;

                    while (currentIndex > 0)
                    {
                        if (targetArray[currentIndex - 1] == null)
                        {
                            currentIndex--;
                            dstIndex = currentIndex;
                        }
                        else if (Equals(targetArray[currentIndex - 1].Obj, targetArray[srcIndex].Obj, compare))
                        {
                            currentIndex--;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (dstIndex < 0)
                    {
                        currentIndex = srcIndex;

                        while (currentIndex < max / 2 - 1)
                        {
                            if (targetArray[currentIndex + 1] == null)
                            {
                                currentIndex++;
                                dstIndex = currentIndex;
                            }
                            else if (Equals(targetArray[currentIndex + 1].Obj, targetArray[srcIndex].Obj, compare))
                            {
                                currentIndex++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (dstIndex >= 0)
                    {
                        targetArray[dstIndex] = targetArray[srcIndex];
                        targetArray[srcIndex] = null;
                    }
                }
            }

            var srcList = new List<NullableDiffObject<T>>(srcArray.Length);
            var dstList = new List<NullableDiffObject<T>>(srcList.Capacity);

            var compactedCount = 0;
            for (int i = 0; i < srcArray.Length; i++)
            {
                if (srcArray[i] == null && dstArray[i] == null)
                {
                    compactedCount++;
                    continue;
                }

                srcList.Add(srcArray[i]);
                dstList.Add(dstArray[i]);
            }

            srcArray = srcList.ToArray();
            dstArray = dstList.ToArray();

            return compactedCount;
        }

        private static bool Equals<T>(T x, T y, IEqualityComparer<T> compare = null)
        {
            if (compare != null)
                return compare.Equals(x, y);

            if (x == null)
            {
                if (y == null)
                    return true;
                else
                    return false;
            }

            return x.Equals(y);
        }

        private static IEnumerable<DiffResult<T>> MakeResults<T>(IEnumerable<Point> waypoints, IEnumerable<T> seq1, IEnumerable<T> seq2)
        {
            var array1 = seq1.ToArray();
            var array2 = seq2.ToArray();

            foreach (var pair in waypoints.MakePairsWithNext())
            {
                var status = GetStatus(pair.Item1, pair.Item2);
                T obj1 = default(T);
                T obj2 = default(T);
                switch (status)
                {
                    case DiffStatus.Equal:
                        obj1 = array1[pair.Item2.X - 1];
                        obj2 = array2[pair.Item2.Y - 1];
                        break;
                    case DiffStatus.Inserted:
                        obj2 = array2[pair.Item2.Y - 1];
                        break;
                    case DiffStatus.Deleted:
                        obj1 = array1[pair.Item2.X - 1];
                        break;
                }

                yield return new DiffResult<T>(obj1, obj2, status);
            }
        }

        private static DiffStatus GetStatus(Point current, Point prev)
        {
            if (current.X != prev.X && current.Y != prev.Y)
                return DiffStatus.Equal;
            else if (current.X != prev.X)
                return DiffStatus.Deleted;
            else if (current.Y != prev.Y)
                return DiffStatus.Inserted;
            else
                throw new Exception();
        }

        public static IEnumerable<DiffResult<T>> Order<T>(IEnumerable<DiffResult<T>> results, DiffOrderType orderType)
        {
            var resultQueue = new Queue<DiffResult<T>>(results);
            var additionQueue = new Queue<DiffResult<T>>();
            var deletionQueue = new Queue<DiffResult<T>>();

            while (resultQueue.Any())
            {
                if (resultQueue.Peek().Status == DiffStatus.Equal)
                {
                    yield return resultQueue.Dequeue();
                    continue;
                }

                while (resultQueue.Any() && resultQueue.Peek().Status != DiffStatus.Equal)
                {
                    while (resultQueue.Any() && resultQueue.Peek().Status == DiffStatus.Inserted)
                    {
                        additionQueue.Enqueue(resultQueue.Dequeue());
                    }

                    while (resultQueue.Any() && resultQueue.Peek().Status == DiffStatus.Deleted)
                    {
                        deletionQueue.Enqueue(resultQueue.Dequeue());
                    }
                }

                var latestReturenStatus = DiffStatus.Equal;
                while (true)
                {
                    if (additionQueue.Any() && !deletionQueue.Any())
                    {
                        yield return additionQueue.Dequeue();
                    }
                    else if (!additionQueue.Any() && deletionQueue.Any())
                    {
                        yield return deletionQueue.Dequeue();
                    }
                    else if (additionQueue.Any() && deletionQueue.Any())
                    {
                        switch (orderType)
                        {
                            case DiffOrderType.GreedyDeleteFirst:
                                yield return deletionQueue.Dequeue();
                                latestReturenStatus = DiffStatus.Deleted;
                                break;
                            case DiffOrderType.GreedyInsertFirst:
                                yield return additionQueue.Dequeue();
                                latestReturenStatus = DiffStatus.Inserted;
                                break;
                            case DiffOrderType.LazyDeleteFirst:
                                if (latestReturenStatus != DiffStatus.Deleted)
                                {
                                    yield return deletionQueue.Dequeue();
                                    latestReturenStatus = DiffStatus.Deleted;
                                }
                                else
                                {
                                    yield return additionQueue.Dequeue();
                                    latestReturenStatus = DiffStatus.Inserted;
                                }
                                break;
                            case DiffOrderType.LazyInsertFirst:
                                if (latestReturenStatus != DiffStatus.Inserted)
                                {
                                    yield return additionQueue.Dequeue();
                                    latestReturenStatus = DiffStatus.Inserted;
                                }
                                else
                                {
                                    yield return deletionQueue.Dequeue();
                                    latestReturenStatus = DiffStatus.Deleted;
                                }
                                break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }

    class NullableDiffObject<T>
    {
        public T Obj { get; }

        public NullableDiffObject(T obj)
        {
            Obj = obj;
        }
    }
}
