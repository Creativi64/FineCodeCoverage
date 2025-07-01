using System;
using System.Collections.Generic;

namespace FineCodeCoverage.Core.Utilities
{
    public static class IReadOnlyListExtensions
    {
        public static void ForEach<T>(this IReadOnlyList<T> list, Action<T> action)
        {
            foreach (T item in list)
            {
                action(item);
            }
        }
    }
}
