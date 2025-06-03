using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(INewCodeTrackerFactory))]
    internal class NewCodeTrackerFactory : INewCodeTrackerFactory
    {
        private readonly ITrackedNewCodeLineFactory trackedNewCodeLineFactory;
        private readonly ITextInfoFactory textInfoFactory;

        [ImportingConstructor]
        public NewCodeTrackerFactory(
            ITrackedNewCodeLineFactory trackedNewCodeLineFactory,
            ITextInfoFactory textInfoFactory
        )
        {
            this.trackedNewCodeLineFactory = trackedNewCodeLineFactory;
            this.textInfoFactory = textInfoFactory;
        }

        public INewCodeTracker Create(ILineExcluder lineExcluder) => new NewCodeTracker(this.trackedNewCodeLineFactory, lineExcluder, this.textInfoFactory);
    }
}