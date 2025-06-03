using System;

namespace FineCodeCoverage.Editor.IndicatorVisibility
{
    internal interface IFileIndicatorVisibility
    {
        event EventHandler VisibilityChanged;
        bool IsVisible(string filePath);
    }
}