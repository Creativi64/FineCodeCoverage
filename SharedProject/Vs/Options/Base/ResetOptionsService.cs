using System.Collections.Generic;
using System.ComponentModel.Composition;
using FineCodeCoverage.Engine;

namespace FineCodeCoverage.Options
{
    [Export(typeof(ResetOptionsService))]
    internal sealed class ResetOptionsService
    {
        private readonly IEnumerable<IResetOptions> _resetters;
        private readonly IMessageBox _messageBox;

        [ImportingConstructor]
        public ResetOptionsService(
            [ImportMany(typeof(IResetOptions))] IEnumerable<IResetOptions> resetters,
            IMessageBox messageBox)
        {
            _resetters = resetters;
            _messageBox = messageBox;
        }

        public void Reset()
        {
            if (!_messageBox.ShowWarning(
                "Are you sure you want to reset all settings to their default values?",
                "Reset Settings"))
            {
                return;
            }

            foreach (IResetOptions resetter in _resetters)
            {
                resetter.Reset();
            }
        }
    }
}
