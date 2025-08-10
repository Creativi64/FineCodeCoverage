using FineCodeCoverage.Editor.DynamicCoverage.Common;
using Microsoft.VisualStudio.Text.Formatting;

namespace FineCodeCoverage.Editor.Management
{
    internal sealed class CoverageTypeColour : ICoverageTypeColour
    {
        public CoverageTypeColour(DynamicCoverageType coverageType, TextFormattingRunProperties textFormattingRunProperties)
        {
            CoverageType = coverageType;
            TextFormattingRunProperties = textFormattingRunProperties;
        }

        public DynamicCoverageType CoverageType { get; }

        public TextFormattingRunProperties TextFormattingRunProperties { get; }
    }
}
