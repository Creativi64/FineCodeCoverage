using System.Collections.Generic;
using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Editor.Tagging.Base;
using FineCodeCoverage.Options.EditorCoverageColouring;

namespace FineCodeCoverage.Editor.Tagging.GlyphMargin
{
    internal sealed class GlyphFilter : CoverageTypeFilterBase
    {
        public override string TypeIdentifier => "Glyph";

        protected override bool Enabled(EditorCoverageColouringOptions editorCoverageColouringOptions)
            => editorCoverageColouringOptions.ShowCoverageInGlyphMargin;

        protected override Dictionary<DynamicCoverageType, bool> GetShowLookup(EditorCoverageColouringOptions editorCoverageColouringOptions)
            => new Dictionary<DynamicCoverageType, bool>
            {
                { DynamicCoverageType.Covered, editorCoverageColouringOptions.ShowCoveredInGlyphMargin },
                { DynamicCoverageType.Partial, editorCoverageColouringOptions.ShowPartiallyCoveredInGlyphMargin },
                { DynamicCoverageType.NotCovered, editorCoverageColouringOptions.ShowUncoveredInGlyphMargin },
                { DynamicCoverageType.Dirty, editorCoverageColouringOptions.ShowDirtyInGlyphMargin },
                { DynamicCoverageType.NewLine, editorCoverageColouringOptions.ShowNewInGlyphMargin },
                { DynamicCoverageType.NotIncluded, editorCoverageColouringOptions.ShowNotIncludedInGlyphMargin },
            };
    }
}
