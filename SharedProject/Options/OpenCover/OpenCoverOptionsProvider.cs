using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Options
{
    [Export(typeof(IOptionsProvider<OpenCoverOptions>))]
    [Export(typeof(IDialogPageOptionsProvider<OpenCoverOptions>))]
    [Export(typeof(IProfileOptionsProvider))]
    [Export(typeof(IResetOptions))]
    internal class OpenCoverOptionsProvider : OptionsProviderBase<OpenCoverOptions>
    {
        [ImportingConstructor]
        public OpenCoverOptionsProvider(
                ILogger logger,
                IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider,
                IJsonConvertService jsonConvertService,
                IDefaultOptionsSetter<OpenCoverOptions> defaultOptionsSetter)
            : base(
                logger, writableUserSettingsStoreProvider, jsonConvertService, defaultOptionsSetter)
        {
        }
    }
}
