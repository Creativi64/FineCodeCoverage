using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(INewCodeTrackerFactory))]
    internal class NewCodeTrackerFactory : INewCodeTrackerFactory
    {
        private readonly ITrackedNewCodeLineFactory _trackedNewCodeLineFactory;
        private readonly ITextInfoFactory _textInfoFactory;

        [ImportingConstructor]
        public NewCodeTrackerFactory(
            ITrackedNewCodeLineFactory trackedNewCodeLineFactory,
            ITextInfoFactory textInfoFactory)
        {
            _trackedNewCodeLineFactory = trackedNewCodeLineFactory;
            _textInfoFactory = textInfoFactory;
        }

        public INewCodeTracker Create(ILineExcluder lineExcluder)
            => new NewCodeTracker(_trackedNewCodeLineFactory, lineExcluder, _textInfoFactory);
    }
}
