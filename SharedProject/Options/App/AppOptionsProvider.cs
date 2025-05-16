using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Options
{
    [Export(typeof(IOptionsProvider<AppOptions>))]
    [Export(typeof(IRequireDialogPageInstantiator))]
    [Export(typeof(IDialogPageOptionsProvider<AppOptions>))]
    [Export(typeof(IProfileOptionsProvider))]
    internal class AppOptionsProvider : OptionsProviderBase<AppOptions>
    {
        [ImportingConstructor]
        public AppOptionsProvider(
            ILogger logger,
            IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider,
            IJsonConvertService jsonConvertService,
            IDefaultOptionsSetter<AppOptions> defaultOptionsSetter
        ) : base(logger, writableUserSettingsStoreProvider, jsonConvertService, defaultOptionsSetter) { }
    }
}
