using System;
using System.Windows.Media;

namespace FineCodeCoverage.Output
{
    internal interface IIconsOptions
    {
        event EventHandler ShowIconsChanged;
        event EventHandler IconSizeChanged;
        bool ShowIcons { get; }
        int IconSize { get; }
        bool Monochrome { get; }
        Color MonochromeColor { get; }
    }
}
