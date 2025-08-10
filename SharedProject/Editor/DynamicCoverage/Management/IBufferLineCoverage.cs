using System.Collections.Generic;
using FineCodeCoverage.Editor.DynamicCoverage.Common;

namespace FineCodeCoverage.Editor.DynamicCoverage.Management
{
    internal interface IBufferLineCoverage
    {
        IEnumerable<IDynamicLine> GetLines(int startLineNumber, int endLineNumber);

        void SetLastCoverage(ILastCoverage lastCoverage);

        bool HasCoverage { get; }
    }
}
