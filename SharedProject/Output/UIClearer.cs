using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Engine.Messages;
using FineCodeCoverage.Options;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output
{
    [Export(typeof(IUIClearer))]
    internal class UIClearer : IUIClearer, IListener<CoverageStartingMessage>
    {
        private readonly IEventAggregator eventAggregator;

        [ImportingConstructor]
        public UIClearer(
            ISolutionEvents solutionEvents,
            IAppOptionsProvider appOptionsProvider,
            IEventAggregator eventAggregator
            )
        {
            eventAggregator.AddListener(this);
            solutionEvents.AfterClosing += (s, args) => ClearUI();
            appOptionsProvider.OptionsChanged += (appOptions) =>
            {
                if (!appOptions.Enabled)
                {
                    ClearUI();
                }
            };
            this.eventAggregator = eventAggregator;
        }

        public void ClearUI()
        {
            eventAggregator.SendMessage(new ClearReportMessage());
            eventAggregator.SendMessage(new ClearLinesMessage());
        }

        public void Handle(CoverageStartingMessage message)
        {
            eventAggregator.SendMessage(new ClearLinesMessage());
        }
    }

}
