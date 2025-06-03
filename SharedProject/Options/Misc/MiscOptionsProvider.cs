using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Options.Tools
{
    [Export(typeof(IOptionsProvider<MiscOptions>))]
    [Export(typeof(IDialogPageOptionsProvider<MiscOptions>))]
    [Export(typeof(IProfileOptionsProvider))]
    [Export(typeof(IResetOptions))]
    internal class MiscOptionsProvider : OptionsProviderBase<MiscOptions>
    {
        [ImportingConstructor]
        public MiscOptionsProvider(
                ILogger logger,
                IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider,
                IJsonConvertService jsonConvertService,
                IDefaultOptionsSetter<MiscOptions> defaultOptionsSetter
            ) : base(
                logger, writableUserSettingsStoreProvider, jsonConvertService, defaultOptionsSetter
            )
        { }
    }
}