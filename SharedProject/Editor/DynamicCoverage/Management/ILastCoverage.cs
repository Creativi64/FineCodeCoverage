using System;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface ILastCoverage
    {
        IFileLineCoverage FileLineCoverage { get; }

        DateTime TestExecutionStartingDate { get; }
    }
}
