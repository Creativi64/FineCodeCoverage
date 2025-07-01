using System.ComponentModel.Composition;
using FineCodeCoverage.Options.Base;
using FineCodeCoverage.Output;
using FineCodeCoverage.Utilities.Wrappers;

namespace FineCodeCoverage.Options.Coverlet
{
    [Export(typeof(IOptionsProvider<CoverletOptions>))]
    [Export(typeof(IDialogPageOptionsProvider<CoverletOptions>))]
    [Export(typeof(IProfileOptionsProvider))]
    [Export(typeof(IResetOptions))]
    internal sealed class CoverletOptionsProvider : OptionsProviderBase<CoverletOptions>
    {
        [ImportingConstructor]
        public CoverletOptionsProvider(
                ILogger logger,
                IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider,
                IJsonConvertService jsonConvertService,
                IDefaultOptionsSetter<CoverletOptions> defaultOptionsSetter)
            : base(
                logger, writableUserSettingsStoreProvider, jsonConvertService, defaultOptionsSetter)
        {
        }
    }
}
