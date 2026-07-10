using FineCodeCoverage.Editor.DynamicCoverage.Management;

namespace FineCodeCoverage.Editor.DynamicCoverage.Messages
{
    internal sealed class NewCoverageLinesMessage
    {
        public NewCoverageLinesMessage(IFileLineCoverage coverageLines) => CoverageLines = coverageLines;

        public IFileLineCoverage CoverageLines { get; }
    }
}
