using System;

namespace FineCodeCoverage.Output
{
    interface IShowIcons
    {
        event EventHandler ShowIconsChanged;
        bool ShowIcons { get; }
    }
}
