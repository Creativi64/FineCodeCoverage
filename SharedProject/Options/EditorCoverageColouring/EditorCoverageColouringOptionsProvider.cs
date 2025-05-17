using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Options
{
    [Export(typeof(IOptionsProvider<EditorCoverageColouringOptions>))]
    [Export(typeof(IDialogPageOptionsProvider<EditorCoverageColouringOptions>))]
    [Export(typeof(IProfileOptionsProvider))]
    internal class EditorCoverageColouringOptionsProvider : OptionsProviderBase<EditorCoverageColouringOptions>
    {
        [ImportingConstructor]
        public EditorCoverageColouringOptionsProvider(
            ILogger logger,
            IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider,
            IJsonConvertService jsonConvertService,
            IDefaultOptionsSetter<EditorCoverageColouringOptions> defaultOptionsSetter
        ) : base(logger, writableUserSettingsStoreProvider, jsonConvertService, defaultOptionsSetter) { }
    }
}
