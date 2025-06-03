using System.Collections.Generic;
using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Editor.Tagging.Base;
using FineCodeCoverage.Options;

namespace FineCodeCoverage.Editor.Tagging.Classification
{
    internal class CoverageClassificationFilter : CoverageTypeFilterBase
    {
        public override string TypeIdentifier => "Classification";

        protected override bool Enabled(EditorCoverageColouringOptions editorCoverageColouringOptions) => editorCoverageColouringOptions.ShowLineCoverageHighlighting;

        protected override Dictionary<DynamicCoverageType, bool> GetShowLookup(EditorCoverageColouringOptions editorCoverageColouringOptions)
            => new Dictionary<DynamicCoverageType, bool>()
            {
                { DynamicCoverageType.Covered, editorCoverageColouringOptions.ShowLineCoveredHighlighting },
                { DynamicCoverageType.Partial, editorCoverageColouringOptions.ShowLinePartiallyCoveredHighlighting },
                { DynamicCoverageType.NotCovered, editorCoverageColouringOptions.ShowLineUncoveredHighlighting },
                { DynamicCoverageType.Dirty, editorCoverageColouringOptions.ShowLineDirtyHighlighting },
                { DynamicCoverageType.NewLine, editorCoverageColouringOptions.ShowLineNewHighlighting },
                { DynamicCoverageType.NotIncluded, editorCoverageColouringOptions.ShowLineNotIncludedHighlighting },
            };
    }
}