using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Options
{
    [Export(typeof(IOptionsProvider<HotspotThresholdsOptions>))]
    [Export(typeof(IDialogPageOptionsProvider<HotspotThresholdsOptions>))]
    [Export(typeof(IProfileOptionsProvider))]
    [Export(typeof(IResetOptions))]
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
            )
        { }

    }
}
