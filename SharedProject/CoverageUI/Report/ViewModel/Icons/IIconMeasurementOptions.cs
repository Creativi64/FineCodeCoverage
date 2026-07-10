using System;

namespace FineCodeCoverage.Output
{
    internal interface IIconMeasurementOptions
    {
        event EventHandler ShowIconsChanged;

        event EventHandler IconSizeChanged;

        bool ShowIcons { get; }

        int IconSize { get; }
    }
}
