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
        private readonly IEventAggregator _eventAggregator;

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
            this._eventAggregator = eventAggregator;
        }

        public void ClearUI()
        {
            this._eventAggregator.SendMessage(new ClearReportMessage());
            this._eventAggregator.SendMessage(new ClearLinesMessage());
        }

        public void Handle(CoverageStartingMessage message) => this._eventAggregator.SendMessage(new ClearLinesMessage());
    }
}