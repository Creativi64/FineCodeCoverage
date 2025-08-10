using FineCodeCoverage.Editor.DynamicCoverage.Common;
using Microsoft.VisualStudio.Text.Formatting;

namespace FineCodeCoverage.Editor.Management
{
    internal interface ICoverageTypeColour
    {
        DynamicCoverageType CoverageType { get; }

        TextFormattingRunProperties TextFormattingRunProperties { get; }
    }
}
