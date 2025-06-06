using System;
using System.Diagnostics.CodeAnalysis;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    internal sealed class LastCoverage : ILastCoverage

#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        public LastCoverage(IFileLineCoverage fileLineCoverage, DateTime testExecutionStartingDate)
        {
            FileLineCoverage = fileLineCoverage;
            TestExecutionStartingDate = testExecutionStartingDate;
        }

        public IFileLineCoverage FileLineCoverage { get; }

        public DateTime TestExecutionStartingDate { get; }

        [ExcludeFromCodeCoverage]
        public override bool Equals(object obj) => obj is LastCoverage coverage && FileLineCoverage == coverage.FileLineCoverage && TestExecutionStartingDate == coverage.TestExecutionStartingDate;
    }
}
