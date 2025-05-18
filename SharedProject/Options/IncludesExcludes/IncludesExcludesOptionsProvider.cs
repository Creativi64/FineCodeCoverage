using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Options
{
    [Export(typeof(IOptionsProvider<IncludesExcludesOptions>))]
    [Export(typeof(IDialogPageOptionsProvider<IncludesExcludesOptions>))]
    [Export(typeof(IProfileOptionsProvider))]
    [Export(typeof(IResetOptions))]
    internal class IncludesExcludesOptionsProvider : OptionsProviderBase<IncludesExcludesOptions>
    {
        [ImportingConstructor]
        public IncludesExcludesOptionsProvider(
            ILogger logger,
            IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider,
            IJsonConvertService jsonConvertService,
            IDefaultOptionsSetter<IncludesExcludesOptions> defaultOptionsSetter
        ) : base(logger, writableUserSettingsStoreProvider, jsonConvertService, defaultOptionsSetter) { }
    }
}
