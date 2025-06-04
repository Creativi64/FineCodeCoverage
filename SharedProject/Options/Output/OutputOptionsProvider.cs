using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Options
{
    [Export(typeof(IOptionsProvider<OutputOptions>))]
    [Export(typeof(IDialogPageOptionsProvider<OutputOptions>))]
    [Export(typeof(IProfileOptionsProvider))]
    [Export(typeof(IResetOptions))]
    internal class OutputOptionsProvider : OptionsProviderBase<OutputOptions>
    {
        [ImportingConstructor]
        public OutputOptionsProvider(
                ILogger logger,
                IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider,
                IJsonConvertService jsonConvertService,
                IDefaultOptionsSetter<OutputOptions> defaultOptionsSetter
            ) : base(
                logger, writableUserSettingsStoreProvider, jsonConvertService, defaultOptionsSetter
            )
        { }
    }
}
