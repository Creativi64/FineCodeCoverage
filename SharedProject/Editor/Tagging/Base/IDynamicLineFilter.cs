using System;
using FineCodeCoverage.Editor.DynamicCoverage.Common;

namespace FineCodeCoverage.Editor.Tagging.Base
{
    internal interface IDynamicLineFilter
    {
        event EventHandler FilterChanged;

        Func<IDynamicLine, bool> GetFileFilter(string filePath);
    }
}
