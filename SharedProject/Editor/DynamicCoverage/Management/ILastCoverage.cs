using System;

namespace FineCodeCoverage.Editor.DynamicCoverage.Management
{
    internal interface ILastCoverage
    {
        IFileLineCoverage FileLineCoverage { get; }

        DateTime TestExecutionStartingDate { get; }
    }
}
