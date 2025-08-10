using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using FineCodeCoverage.Editor.DynamicCoverage.ContentTypes;
using FineCodeCoverage.Editor.DynamicCoverage.NewCode;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(IContainingCodeTrackedLinesFactory))]
    internal sealed class ContainingCodeTrackedLinesFactory : IContainingCodeTrackedLinesFactory
    {
        public IContainingCodeTrackerTrackedLines Create(
            List<IContainingCodeTracker> containingCodeTrackers,
            INewCodeTracker newCodeTracker,
            IFileCodeSpanRangeService fileCodeSpanRangeService)
            => new TrackedLines(containingCodeTrackers, newCodeTracker, fileCodeSpanRangeService);
    }
}
