using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Editor.IndicatorVisibility;
using FineCodeCoverage.Utilities.Events;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICommandInitializer))]
    internal sealed class ToggleCoverageIndicatorsCommand : CommandInitializerBase
    {
        private readonly IEventAggregator _eventAggregator;

        protected override int CommandId { get; } = PackageIds.cmdidToggleCoverageIndicatorsCommand;

        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public ToggleCoverageIndicatorsCommand(IEventAggregator eventAggregator) => _eventAggregator = eventAggregator;

        protected override void Execute(object sender, EventArgs e)
            => _eventAggregator.SendMessage(new ToggleCoverageIndicatorsMessage());
    }
}
