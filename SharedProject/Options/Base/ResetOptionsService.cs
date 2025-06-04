using System.Collections.Generic;
using System.ComponentModel.Composition;
using FineCodeCoverage.Engine;

namespace FineCodeCoverage.Options
{
    [Export(typeof(ResetOptionsService))]
    internal class ResetOptionsService
    {
        private readonly IEnumerable<IResetOptions> _resetters;
        private readonly IMessageBox _messageBox;

        [ImportingConstructor]
        public ResetOptionsService(
            [ImportMany(typeof(IResetOptions))] IEnumerable<IResetOptions> resetters,
            IMessageBox messageBox
        )
        {
            this._resetters = resetters;
            this._messageBox = messageBox;
        }

        public void Reset()
        {
            if (!this._messageBox.ShowWarning(
                "Are you sure you want to reset all settings to their default values?",
                "Reset Settings"
            ))
            {
                return;
            }

            foreach (IResetOptions resetter in this._resetters)
            {
                resetter.Reset();
            }
        }
    }
}