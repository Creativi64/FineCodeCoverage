using System;

namespace FineCodeCoverage.Output
{
    interface IIconsOptions
    {
        event EventHandler ShowIconsChanged;
        event EventHandler IconSizeChanged;
        event EventHandler ThemedMonochromeIconsChanged;
        bool ShowIcons { get; }
        int IconSize { get; }
        bool ThemedMonochromeIcons { get; }
    }
}
