using System.ComponentModel.Composition;
using FineCodeCoverage.Options.Base;
using FineCodeCoverage.Utilities.Wrappers;
using FineCodeCoverage.VSAbstractions.OutputWindow;
using FineCodeCoverage.VSAbstractions.Store;

namespace FineCodeCoverage.Options.IncludesExcludes
{
    [Export(typeof(IOptionsProvider<IncludesExcludesOptions>))]
    [Export(typeof(IDialogPageOptionsProvider<IncludesExcludesOptions>))]
    [Export(typeof(IProfileOptionsProvider))]
    [Export(typeof(IResetOptions))]
    internal sealed class IncludesExcludesOptionsProvider : OptionsProviderBase<IncludesExcludesOptions>
    {
        [ImportingConstructor]
        public IncludesExcludesOptionsProvider(
            ILogger logger,
            IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider,
            IJsonConvertService jsonConvertService,
            IDefaultOptionsSetter<IncludesExcludesOptions> defaultOptionsSetter)
            : base(logger, writableUserSettingsStoreProvider, jsonConvertService, defaultOptionsSetter)
        {
        }
    }
}
