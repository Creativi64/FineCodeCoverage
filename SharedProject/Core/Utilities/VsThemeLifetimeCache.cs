using System;
using Microsoft.VisualStudio.PlatformUI;

namespace FineCodeCoverage.Core.Utilities
{
    public class VsThemeLifetimeCache<TKey, TValue> where TValue : class
    {
        public VsThemeLifetimeCache()
        {
            VSColorTheme.ThemeChanged += (_) => _cache.Clear();
        }

        private readonly WeakCache<TKey, TValue> _cache = new WeakCache<TKey, TValue>();
        public TValue GetOrAdd(TKey key, Func<TValue> valueFactory)
        {
            return _cache.GetOrAdd(key, valueFactory);
        }
    }
}
