using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Options
{
    [Export(typeof(IOptionsProvider<OutputOptions>))]
    [Export(typeof(IRequireDialogPageInstantiator))]
    [Export(typeof(IDialogPageOptionsProvider<OutputOptions>))]
    [Export(typeof(IProfileOptionsProvider))]
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
