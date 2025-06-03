using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Engine.Messages;
using FineCodeCoverage.Options;

namespace FineCodeCoverage.Output
{
    [Export(typeof(IUIClearer))]
    internal class UIClearer : IUIClearer, IListener<CoverageStartingMessage>
    {
        private readonly IEventAggregator eventAggregator;

        [ImportingConstructor]
        public UIClearer(
            ISolutionEvents solutionEvents,
            IOptionsProvider<RunOptions> runOptionsProvider,
            IEventAggregator eventAggregator
            )
        {
            _ = eventAggregator.AddListener(this);
            solutionEvents.AfterClosing += (s, args) => this.ClearUI();
            runOptionsProvider.OptionsChanged += (runOptions) =>
            {
                if (!runOptions.Enabled)
                {
                    this.ClearUI();
                }
            };
            this.eventAggregator = eventAggregator;
        }

        public void ClearUI()
        {
            this.eventAggregator.SendMessage(new ClearReportMessage());
            this.eventAggregator.SendMessage(new ClearLinesMessage());
        }

        public void Handle(CoverageStartingMessage message) => this.eventAggregator.SendMessage(new ClearLinesMessage());
    }
}