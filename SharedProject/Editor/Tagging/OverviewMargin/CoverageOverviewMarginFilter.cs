using System.Collections.Generic;
using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Editor.DynamicCoverage.Common;
using FineCodeCoverage.Editor.Tagging.Base;
using FineCodeCoverage.Options.EditorCoverageColouring;

namespace FineCodeCoverage.Editor.Tagging.OverviewMargin
{
    internal sealed class CoverageOverviewMarginFilter : CoverageTypeFilterBase
    {
        public override string TypeIdentifier => "OverviewMargin";

        protected override bool Enabled(EditorCoverageColouringOptions editorCoverageColouringOptions)
            => editorCoverageColouringOptions.ShowCoverageInOverviewMargin;

        protected override Dictionary<DynamicCoverageType, bool> GetShowLookup(
            EditorCoverageColouringOptions editorCoverageColouringOptions) => new Dictionary<DynamicCoverageType, bool>
            {
                { DynamicCoverageType.Covered, editorCoverageColouringOptions.ShowCoveredInOverviewMargin },
                { DynamicCoverageType.NotCovered, editorCoverageColouringOptions.ShowUncoveredInOverviewMargin },
                { DynamicCoverageType.Partial, editorCoverageColouringOptions.ShowPartiallyCoveredInOverviewMargin },
                { DynamicCoverageType.Dirty, editorCoverageColouringOptions.ShowDirtyInOverviewMargin },
                { DynamicCoverageType.NewLine, editorCoverageColouringOptions.ShowNewInOverviewMargin },
                { DynamicCoverageType.NotIncluded, editorCoverageColouringOptions.ShowNotIncludedInOverviewMargin },
            };
    }
}
