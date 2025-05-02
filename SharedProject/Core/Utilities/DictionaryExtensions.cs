using System;
using System.Collections.Generic;
using System.Text;

namespace FineCodeCoverage.Core.Utilities
{
    internal static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(
            this IDictionary<TKey, TValue> dict,
            TKey key,
            Func<TValue> factory)
        {
            if (!dict.TryGetValue(key, out var value))
            {
                value = factory();
                dict.Add(key, value);
            }
            return value;
        }
    }
}
