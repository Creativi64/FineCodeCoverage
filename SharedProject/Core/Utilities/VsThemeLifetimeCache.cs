using System;
using Microsoft.VisualStudio.PlatformUI;

namespace FineCodeCoverage.Core.Utilities
{
    public class VsThemeLifetimeCache<TKey, TValue> where TValue : class
    {
        private readonly WeakCache<TKey, TValue> _cache = new WeakCache<TKey, TValue>();

        public VsThemeLifetimeCache() => VSColorTheme.ThemeChanged += (_) => this._cache.Clear();

        public TValue GetOrAdd(TKey key, Func<TValue> valueFactory) => this._cache.GetOrAdd(key, valueFactory);
    }
}
