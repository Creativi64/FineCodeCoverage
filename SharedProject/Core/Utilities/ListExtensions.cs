using System;
using System.Collections.Generic;

namespace FineCodeCoverage.Core.Utilities
{
    public static class List
    {
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            foreach (T item in items)
            {
                list.Add(item);
            }
        }

        public static bool AddIfNotNull<T>(this IList<T> list, T item)
        {
            if (item != null)
            {
                list.Add(item);
                return true;
            }

            return false;
        }

        // To be performed on a sorted list
        // Returns -1 for empty list or when all elements are outside the lower bounds
        // Compare fn to return 0 for element considered the lower bound
        // > 0 for lower bound greater than element

        public static int LowerBound<T>(this IList<T> list, Func<T, int> compare)
        {
            int first = 0;
            int count = list.Count;
            if (count == 0) return -1;

            while (count > 0)
            {
                int step = count / 2;
                int index = first + step;
                int result = compare(list[index]);
                if (result == 0)
                {
                    return index;
                }
                else if (result > 0)
                {
                    first = ++index;
                    count -= step + 1;
                }
                else
                {
                    count = step;
                }
            }

            return first != list.Count ? first : -1;
        }
    }
}