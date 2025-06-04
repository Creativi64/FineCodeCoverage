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
            ITextInfoFactory textInfoFactory
        )
        {
            this._trackedNewCodeLineFactory = trackedNewCodeLineFactory;
            this._textInfoFactory = textInfoFactory;
        }

        public INewCodeTracker Create(ILineExcluder lineExcluder)
            => new NewCodeTracker(this._trackedNewCodeLineFactory, lineExcluder, this._textInfoFactory);
    }
}
