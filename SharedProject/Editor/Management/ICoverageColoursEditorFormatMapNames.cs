using FineCodeCoverage.Editor.DynamicCoverage.Common;

namespace FineCodeCoverage.Editor.Management
{
    internal interface ICoverageColoursEditorFormatMapNames
    {
        string GetEditorFormatDefinitionName(DynamicCoverageType coverageType);
    }
}
