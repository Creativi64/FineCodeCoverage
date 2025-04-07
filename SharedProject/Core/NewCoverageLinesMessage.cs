using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine
{
    internal sealed class NewCoverageLinesMessage
    {
        public NewCoverageLinesMessage(IFileLineCoverage coverageLines)
        {
            CoverageLines = coverageLines;
        }

        public IFileLineCoverage CoverageLines { get; }
    }
}
