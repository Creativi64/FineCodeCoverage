using System.ComponentModel.Composition;
using FineCodeCoverage.Options.Base;
using FineCodeCoverage.Utilities.Wrappers;
using FineCodeCoverage.VSAbstractions.OutputWindow;
using FineCodeCoverage.VSAbstractions.Store;

namespace FineCodeCoverage.Options.Hotspots
{
    [Export(typeof(IOptionsProvider<HotspotThresholdsOptions>))]
    [Export(typeof(IDialogPageOptionsProvider<HotspotThresholdsOptions>))]
    [Export(typeof(IProfileOptionsProvider))]
    [Export(typeof(IResetOptions))]
    internal sealed class HotspotThresholdsOptionsProvider : OptionsProviderBase<HotspotThresholdsOptions>
    {
        [ImportingConstructor]
        public HotspotThresholdsOptionsProvider(
                ILogger logger,
                IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider,
                IJsonConvertService jsonConvertService,
                IDefaultOptionsSetter<HotspotThresholdsOptions> defaultOptionsSetter)
            : base(
                logger, writableUserSettingsStoreProvider, jsonConvertService, defaultOptionsSetter)
        {
        }
    }
}
