using System.ComponentModel.Composition;
using FineCodeCoverage.Options.Base;
using FineCodeCoverage.Utilities.Wrappers;
using FineCodeCoverage.VSAbstractions.OutputWindow;
using FineCodeCoverage.VSAbstractions.Store;

namespace FineCodeCoverage.Options.Output
{
    [Export(typeof(IOptionsProvider<OutputOptions>))]
    [Export(typeof(IDialogPageOptionsProvider<OutputOptions>))]
    [Export(typeof(IProfileOptionsProvider))]
    [Export(typeof(IResetOptions))]
    internal sealed class OutputOptionsProvider : OptionsProviderBase<OutputOptions>
    {
        [ImportingConstructor]
        public OutputOptionsProvider(
                ILogger logger,
                IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider,
                IJsonConvertService jsonConvertService,
                IDefaultOptionsSetter<OutputOptions> defaultOptionsSetter)
            : base(
                logger, writableUserSettingsStoreProvider, jsonConvertService, defaultOptionsSetter)
        {
        }
    }
}
