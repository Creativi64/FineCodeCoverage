using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Options.Tools
{
    [Export(typeof(IOptionsProvider<ToolsOptions>))]
    [Export(typeof(IRequireDialogPageInstantiator))]
    [Export(typeof(IDialogPageOptionsProvider<ToolsOptions>))]
    [Export(typeof(IProfileOptionsProvider))]
    internal class ToolsOptionsProvider : OptionsProviderBase<ToolsOptions>
    {
        [ImportingConstructor]
        public ToolsOptionsProvider(
                ILogger logger,
                IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider,
                IJsonConvertService jsonConvertService,
                IDefaultOptionsSetter<ToolsOptions> defaultOptionsSetter
            ) : base(
                logger, writableUserSettingsStoreProvider, jsonConvertService, defaultOptionsSetter
            )
        { }
    }
}
