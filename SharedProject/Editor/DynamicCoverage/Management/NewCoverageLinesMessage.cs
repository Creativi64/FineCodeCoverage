namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal sealed class NewCoverageLinesMessage
    {
        public NewCoverageLinesMessage(IFileLineCoverage coverageLines) => this.CoverageLines = coverageLines;

        public IFileLineCoverage CoverageLines { get; }
    }
}
