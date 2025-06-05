using System;
using System.Collections.Generic;

namespace FineCodeCoverage.Core.Utilities
{
    internal static class LinqExtensions
    {
        public static TTransformed SelectFirstNonNull<T, TTransformed>(
            this IEnumerable<T> source,
            Func<T, TTransformed> select
        )
            where TTransformed : class
        {
            foreach (T element in source)
            {
                TTransformed selected = select(element);
                if (selected != null)
                {
                    return selected;
                }
            }

            return null;
        }

        public static IEnumerable<T> TakeUntil<T>(this IEnumerable<T> source, Func<T, bool> predicate)
            => source == null
                ? throw new ArgumentNullException(nameof(source))
                : predicate == null ? throw new ArgumentNullException(nameof(predicate)) :
                TakeUntilIterator(source, predicate);

        private static IEnumerable<T> TakeUntilIterator<T>(IEnumerable<T> source, Func<T, bool> predicate)
        {
            foreach (T item in source)
            {
                yield return item;
                if (predicate(item))
                {
                    yield break;
                }
            }
        }
    }
}
