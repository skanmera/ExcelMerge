using System;

namespace SKCore.Collection
{
    public static partial class EnumerableExtension
    {
        public static void Times(this int count, Action action)
        {
            count.Times(0, i => action());
        }

        public static void Times(this int count, Action<int> action)
        {
            count.Times(0, action);
        }

        public static void Times(this int count, int start, Action<int> action)
        {
            if (count > 0)
                for (int i = start, end = start + count; i < end; i++)
                    action(i);
            else
                for (int i = start, end = start + count; i > end; i--)
                    action(i);
        }
    }
}
