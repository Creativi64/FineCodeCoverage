using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Options.Base;
using FineCodeCoverage.Output;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Options
{
    [Export(typeof(IOptionsProvider<CoverletOptions>))]
    [Export(typeof(IDialogPageOptionsProvider<CoverletOptions>))]
    [Export(typeof(IProfileOptionsProvider))]
    [Export(typeof(IResetOptions))]
    internal class CoverletOptionsProvider : OptionsProviderBase<CoverletOptions>
    {
        [ImportingConstructor]
        public CoverletOptionsProvider(
                ILogger logger,
                IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider,
                IJsonConvertService jsonConvertService,
                IDefaultOptionsSetter<CoverletOptions> defaultOptionsSetter
            ) : base(
                logger, writableUserSettingsStoreProvider, jsonConvertService, defaultOptionsSetter
            ) { }

    }
}
