using FineCodeCoverage.Core.Utilities;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Options
{
    internal abstract class DialogPageBase<TOptions> : DialogPage where TOptions : class
    {

        private IDialogPageOptionsProvider<TOptions> _optionsProvider;
        private IDialogPageOptionsProvider<TOptions> OptionsProvider => this._optionsProvider ??
            (this._optionsProvider = MefServiceProvider.Get<IDialogPageOptionsProvider<TOptions>>());

        public override object AutomationObject => this.OptionsProvider.Options;

        public override void SaveSettingsToStorage() => this.OptionsProvider.SaveSettingsToStorage();

        public override void LoadSettingsFromStorage() => this.OptionsProvider.LoadSettingsFromStorage();
    }
}