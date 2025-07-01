using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Options.Base;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Options.Run
{
    [Export(typeof(IOptionsProvider<RunOptions>))]
    [Export(typeof(IDialogPageOptionsProvider<RunOptions>))]
    [Export(typeof(IProfileOptionsProvider))]
    [Export(typeof(IResetOptions))]
    internal sealed class RunOptionsProvider : OptionsProviderBase<RunOptions>
    {
        [ImportingConstructor]
        public RunOptionsProvider(
                ILogger logger,
                IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider,
                IJsonConvertService jsonConvertService,
                IDefaultOptionsSetter<RunOptions> defaultOptionsSetter)
            : base(
                logger, writableUserSettingsStoreProvider, jsonConvertService, defaultOptionsSetter)
        {
        }
    }
}
