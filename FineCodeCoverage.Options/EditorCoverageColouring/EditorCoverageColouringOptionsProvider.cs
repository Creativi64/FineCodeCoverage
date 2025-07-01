using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Options.Base;
using FineCodeCoverage.Output;

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
