using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.IndicatorVisibility;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICommandInitializer))]
    internal sealed class ToggleCoverageIndicatorsCommand : CommandInitializerBase
    {
        private readonly IEventAggregator eventAggregator;

        protected override int CommandId { get; } = PackageIds.cmdidToggleCoverageIndicatorsCommand;
        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public ToggleCoverageIndicatorsCommand(IEventAggregator eventAggregator) => this.eventAggregator = eventAggregator;

        protected override void Execute(object sender, EventArgs e)
            => this.eventAggregator.SendMessage(new ToggleCoverageIndicatorsMessage());
    }
}
