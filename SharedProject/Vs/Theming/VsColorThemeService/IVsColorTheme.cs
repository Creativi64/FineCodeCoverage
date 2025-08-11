using System;

namespace FineCodeCoverage.Vs.Theming.VsColorThemeService
{
    public interface IVsColorTheme
    {
        event EventHandler ThemeChanged;

        VsColorEntry GetColorEntry(ColorName colorName);

        string CurrentThemeName { get; }
    }
}
