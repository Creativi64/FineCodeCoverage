using FineCodeCoverage.Editor.Tagging.OverviewMargin;
using FineCodeCoverage.Options.EditorCoverageColouring;
using FineCodeCoverageTests.Editor.Tagging.CoverageTypeFilter;
using System;
using System.Linq.Expressions;

namespace FineCodeCoverageTests.Editor.Tagging.OverviewMargin
{
    internal class CoverageOverviewMargin_Tests : CoverageTypeFilter_Tests_Base<CoverageOverviewMarginFilter>
    {
        protected override Expression<Func<EditorCoverageColouringOptions, bool>> ShowCoverageExpression { get; } = appOptions => appOptions.ShowCoverageInOverviewMargin;

        protected override Expression<Func<EditorCoverageColouringOptions, bool>> ShowCoveredExpression { get; } = appOptions => appOptions.ShowCoveredInOverviewMargin;

        protected override Expression<Func<EditorCoverageColouringOptions, bool>> ShowUncoveredExpression { get; } = appOptions => appOptions.ShowUncoveredInOverviewMargin;

        protected override Expression<Func<EditorCoverageColouringOptions, bool>> ShowPartiallyCoveredExpression { get; } = appOptions => appOptions.ShowPartiallyCoveredInOverviewMargin;

        protected override Expression<Func<EditorCoverageColouringOptions, bool>> ShowDirtyExpression => appOptions => appOptions.ShowDirtyInOverviewMargin;

        protected override Expression<Func<EditorCoverageColouringOptions, bool>> ShowNewExpression => appOptions => appOptions.ShowNewInOverviewMargin;

        protected override Expression<Func<EditorCoverageColouringOptions, bool>> ShowNotIncludedExpression => appOptions => appOptions.ShowNotIncludedInOverviewMargin;
    }


}