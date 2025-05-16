using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Options
{
    [Export(typeof(IOptionsProvider<RunOptions>))]
    [Export(typeof(IRequireDialogPageInstantiator))]
    [Export(typeof(IDialogPageOptionsProvider<RunOptions>))]
    [Export(typeof(IProfileOptionsProvider))]
    internal class RunOptionsProvider : OptionsProviderBase<RunOptions>
    {
        [ImportingConstructor]
        public RunOptionsProvider(
                ILogger logger,
                IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider,
                IJsonConvertService jsonConvertService,
                IDefaultOptionsSetter<RunOptions> defaultOptionsSetter
            ) : base(
                logger, writableUserSettingsStoreProvider, jsonConvertService, defaultOptionsSetter
            )
        { }

    }
}
