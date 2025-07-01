using System.ComponentModel.Composition;
using FineCodeCoverage.Collection.Messages;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Options;

namespace FineCodeCoverage.Output
{
    [Export(typeof(IUIClearer))]
    internal sealed class UIClearer : IUIClearer, IListener<CoverageStartingMessage>
    {
        private readonly IEventAggregator _eventAggregator;

        [ImportingConstructor]
        public UIClearer(
            ISolutionEvents solutionEvents,
            IOptionsProvider<RunOptions> runOptionsProvider,
            IEventAggregator eventAggregator)
        {
            _ = eventAggregator.AddListener(this);
            solutionEvents.AfterClosing += (s, args) => ClearUI();
            runOptionsProvider.OptionsChanged += (runOptions) =>
            {
                if (runOptions.Enabled)
                {
                    return;
                }

                ClearUI();
            };
            _eventAggregator = eventAggregator;
        }

        public void ClearUI()
        {
            _eventAggregator.SendMessage(new ClearReportMessage());
            _eventAggregator.SendMessage(new ClearLinesMessage());
        }

        public void Handle(CoverageStartingMessage message) => _eventAggregator.SendMessage(new ClearLinesMessage());
    }
}
