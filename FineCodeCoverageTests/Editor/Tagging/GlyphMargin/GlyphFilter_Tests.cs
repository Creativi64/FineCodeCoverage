using FineCodeCoverage.Editor.Tagging.GlyphMargin;
using FineCodeCoverage.Options;
using FineCodeCoverageTests.Editor.Tagging.CoverageTypeFilter;
using System;
using System.Linq.Expressions;

namespace FineCodeCoverageTests.Editor.Tagging.GlyphMargin
{
    internal class GlyphFilter_Tests : CoverageTypeFilter_Tests_Base<GlyphFilter>
    {
        protected override Expression<Func<EditorCoverageColouringOptions, bool>> ShowCoverageExpression { get; } = appOptions => appOptions.ShowCoverageInGlyphMargin;

        protected override Expression<Func<EditorCoverageColouringOptions, bool>> ShowCoveredExpression { get; } = appOptions => appOptions.ShowCoveredInGlyphMargin;

        protected override Expression<Func<EditorCoverageColouringOptions, bool>> ShowUncoveredExpression { get; } = appOptions => appOptions.ShowUncoveredInGlyphMargin;

        protected override Expression<Func<EditorCoverageColouringOptions, bool>> ShowPartiallyCoveredExpression { get; } = appOptions => appOptions.ShowPartiallyCoveredInGlyphMargin;

        protected override Expression<Func<EditorCoverageColouringOptions, bool>> ShowDirtyExpression => appOptions => appOptions.ShowDirtyInGlyphMargin;

        protected override Expression<Func<EditorCoverageColouringOptions, bool>> ShowNewExpression => appOptions => appOptions.ShowNewInGlyphMargin;

        protected override Expression<Func<EditorCoverageColouringOptions, bool>> ShowNotIncludedExpression => appOptions => appOptions.ShowNotIncludedInGlyphMargin;
    }


}