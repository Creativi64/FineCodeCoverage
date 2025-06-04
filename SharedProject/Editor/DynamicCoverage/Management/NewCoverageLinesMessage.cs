namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal sealed class NewCoverageLinesMessage
    {
        public NewCoverageLinesMessage(IFileLineCoverage coverageLines) => CoverageLines = coverageLines;

        public IFileLineCoverage CoverageLines { get; }
    }
}
