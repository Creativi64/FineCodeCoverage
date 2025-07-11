using System.ComponentModel.Composition;
using FineCodeCoverage.Options.Base;
using FineCodeCoverage.Utilities.Wrappers;
using FineCodeCoverage.VSAbstractions.OutputWindow;
using FineCodeCoverage.VSAbstractions.Store;

namespace FineCodeCoverage.Options.Report
{
    [Export(typeof(IOptionsProvider<ReportOptions>))]
    [Export(typeof(IDialogPageOptionsProvider<ReportOptions>))]
    [Export(typeof(IProfileOptionsProvider))]
    [Export(typeof(IResetOptions))]
    internal sealed class ReportOptionsProvider : OptionsProviderBase<ReportOptions>
    {
        [ImportingConstructor]
        public ReportOptionsProvider(
                ILogger logger,
                IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider,
                IJsonConvertService jsonConvertService,
                IDefaultOptionsSetter<ReportOptions> defaultOptionsSetter)
            : base(
                logger, writableUserSettingsStoreProvider, jsonConvertService, defaultOptionsSetter)
        {
        }
    }
}
