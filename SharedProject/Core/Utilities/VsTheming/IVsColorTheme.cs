using System;

namespace FineCodeCoverage.Core.Utilities
{
    public interface IVsColorTheme
    {
        event EventHandler ThemeChanged;
        VsColorEntry GetColorEntry(ColorName colorName);
        string CurrentThemeName { get; }
    }
}
