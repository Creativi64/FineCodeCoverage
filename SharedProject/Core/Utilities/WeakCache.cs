using System;
using System.Collections.Generic;

namespace FineCodeCoverage.Core.Utilities
{
    public class WeakCache<TKey, TValue> where TValue : class
    {
        private readonly Dictionary<TKey, WeakReference<TValue>> _cache = new Dictionary<TKey, WeakReference<TValue>>();

        public TValue GetOrAdd(TKey key, Func<TValue> valueFactory)
        {
            if (this._cache.TryGetValue(key, out WeakReference<TValue> weakRef) && weakRef.TryGetTarget(out TValue existing))
            {
                return existing;
            }

            TValue value = valueFactory();
            this._cache[key] = new WeakReference<TValue>(value);
            return value;
        }

        public void Clear()
        {
            this._cache.Clear();
        }
    }
}
