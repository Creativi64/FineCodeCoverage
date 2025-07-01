using System.ComponentModel.Composition;
using FineCodeCoverage.Options.Base;
using FineCodeCoverage.Output;
using FineCodeCoverage.Utilities.Wrappers;

namespace FineCodeCoverage.Options.EditorCoverageColouring
{
    [Export(typeof(IOptionsProvider<EditorCoverageColouringOptions>))]
    [Export(typeof(IDialogPageOptionsProvider<EditorCoverageColouringOptions>))]
    [Export(typeof(IProfileOptionsProvider))]
    [Export(typeof(IResetOptions))]
    internal sealed class EditorCoverageColouringOptionsProvider : OptionsProviderBase<EditorCoverageColouringOptions>
    {
        [ImportingConstructor]
        public EditorCoverageColouringOptionsProvider(
            ILogger logger,
            IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider,
            IJsonConvertService jsonConvertService,
            IDefaultOptionsSetter<EditorCoverageColouringOptions> defaultOptionsSetter)
            : base(logger, writableUserSettingsStoreProvider, jsonConvertService, defaultOptionsSetter)
        {
        }
    }
}
