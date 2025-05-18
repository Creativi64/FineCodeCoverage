using FineCodeCoverage.Engine;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Options
{
    [Export(typeof(ResetOptionsService))]
    internal class ResetOptionsService
    {
        private readonly IEnumerable<IResetOptions> resetters;
        private readonly IMessageBox messageBox;

        [ImportingConstructor]
        public ResetOptionsService(
            [ImportMany(typeof(IResetOptions))] IEnumerable<IResetOptions> resetters,
            IMessageBox messageBox
        )
        {
            this.resetters = resetters;
            this.messageBox = messageBox;
        }
        public void Reset()
        {
            if (this.messageBox.ShowWarning(
                "Are you sure you want to reset all settings to their default values?",
                "Reset Settings"
            ))
            {
                foreach (var resetter in this.resetters)
                {
                    resetter.Reset();
                }
            }
        }
    }
}
