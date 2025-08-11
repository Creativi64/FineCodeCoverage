using FineCodeCoverage.Options.Base;
using FineCodeCoverage.Utilities.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Vs.Settings.Options
{
    internal abstract class DialogPageBase<TOptions> : DialogPage
        where TOptions : class
    {
        private IDialogPageOptionsProvider<TOptions> _optionsProvider;

        private IDialogPageOptionsProvider<TOptions> OptionsProvider => _optionsProvider ??
            (_optionsProvider = MefServiceProvider.Get<IDialogPageOptionsProvider<TOptions>>());

        public override object AutomationObject => OptionsProvider.Options;

        public override void SaveSettingsToStorage() => OptionsProvider.SaveSettingsToStorage();

        public override void LoadSettingsFromStorage() => OptionsProvider.LoadSettingsFromStorage();
    }
}
