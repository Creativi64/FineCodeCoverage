using System;
using System.Collections.Generic;

namespace FineCodeCoverage.Utilities.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(
            this IDictionary<TKey, TValue> dict,
            TKey key,
            Func<TValue> factory)
        {
            if (!dict.TryGetValue(key, out TValue value))
            {
                value = factory();
                dict.Add(key, value);
            }

            return value;
        }
    }
}
