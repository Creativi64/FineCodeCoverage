using System;
using FineCodeCoverage.Editor.DynamicCoverage;

namespace FineCodeCoverage.Editor.Tagging.Base
{
    interface IDynamicLineFilter
    {
        event EventHandler FilterChanged;

        Func<IDynamicLine, bool> GetFileFilter(string filePath);
    }
}
