using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Options
{
    [Export(typeof(IOptionsProvider<HotspotThresholdsOptions>))]
    [Export(typeof(IRequireDialogPageInstantiator))]
    [Export(typeof(IDialogPageOptionsProvider<HotspotThresholdsOptions>))]
    [Export(typeof(IProfileOptionsProvider))]
    internal class HotspotThresholdsOptionsProvider : OptionsProviderBase<HotspotThresholdsOptions>
    {
        [ImportingConstructor]
        public HotspotThresholdsOptionsProvider(
                ILogger logger,
                IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider,
                IJsonConvertService jsonConvertService,
                IDefaultOptionsSetter<HotspotThresholdsOptions> defaultOptionsSetter
            ) : base(
                logger, writableUserSettingsStoreProvider, jsonConvertService, defaultOptionsSetter
            ) { }

    }
}
