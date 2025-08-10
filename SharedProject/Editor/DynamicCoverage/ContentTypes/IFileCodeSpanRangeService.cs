using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage.ContentTypes
{
    internal interface IFileCodeSpanRangeService
    {
        List<CodeSpanRange> GetFileCodeSpanRanges(ITextSnapshot snapshot);
    }
}
