using FineCodeCoverage.Core.Utilities;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Options
{
    internal abstract class DialogPageBase<TOptions> : DialogPage where TOptions : class
    {

        private IDialogPageOptionsProvider<TOptions> optionsProvider;
        private IDialogPageOptionsProvider<TOptions> OptionsProvider
        {
            get
            {
                return optionsProvider ?? (optionsProvider = MefServiceProvider.Get<IDialogPageOptionsProvider<TOptions>>());
            }
        }
        public override object AutomationObject => OptionsProvider.Options;

        public override void SaveSettingsToStorage()
        {
            OptionsProvider.SaveSettingsToStorage();
        }

        public override void LoadSettingsFromStorage()
        {
            OptionsProvider.LoadSettingsFromStorage();
        }
    }

}
