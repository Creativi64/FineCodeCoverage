using System;
using System.Collections.Generic;

namespace FineCodeCoverage.Core.Utilities
{
    internal static class IReadOnlyListExtensions
    {
        public static void ForEach<T>(this IReadOnlyList<T> list, Action<T> action)
        {
            foreach (var item in list)
            {
                action(item);
            }
        }
    }
}
