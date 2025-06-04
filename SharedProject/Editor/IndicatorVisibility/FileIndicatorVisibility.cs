using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Initialization;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Editor.IndicatorVisibility
{
    [Export(typeof(IInitializable))]
    [Export(typeof(IFileIndicatorVisibility))]
    internal class FileIndicatorVisibility : IFileIndicatorVisibility, IListener<ToggleCoverageIndicatorsMessage>, IInitializable
    {
        private bool _showIndicators = true;
        public event EventHandler VisibilityChanged;

        [ImportingConstructor]
        public FileIndicatorVisibility(IEventAggregator eventAggregator)
            => _ = eventAggregator.AddListener(this);

        public bool IsVisible(string filePath) => this._showIndicators;
        public void Handle(ToggleCoverageIndicatorsMessage message)
        {
            this._showIndicators = !this._showIndicators;
            VisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}