using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICommandInitializer))]
    [Export(typeof(IReportToolWindowOpener))]
    internal sealed class OpenReportWindowCommand : CommandInitializerBase, IReportToolWindowOpener
    {
        private readonly IShownReportToolWindowHistory _shownToolWindowHistory;

        protected override int CommandId { get; } = PackageIds.cmdidOpenReportWindowCommand;

        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public OpenReportWindowCommand(IShownReportToolWindowHistory shownToolWindowHistory) => _shownToolWindowHistory = shownToolWindowHistory;

        protected override void Execute(object sender, EventArgs e) => PackageServices.RunAsyncWithExceptionLogging(ShowToolWindowAsync);

        public async Task<ToolWindowPane> ShowToolWindowAsync()
        {
            _shownToolWindowHistory.ShowedToolWindow();
            ToolWindowPane window = await PackageServices.ShowToolWindowAsync(typeof(ReportToolWindow), 0, true, PackageServices.DisposalToken);

            return ReturnOrThrowIfCannotCreateToolWindow(window);
        }

        private static ToolWindowPane ReturnOrThrowIfCannotCreateToolWindow(ToolWindowPane window)
            => (window == null) || (window.Frame == null)
                ? throw new NotSupportedException($"Cannot create '{Vsix.Name}' report window")
                : window;

        async Task IReportToolWindowOpener.TryOpenAsync()
        {
            try
            {
                _ = await ShowToolWindowAsync();
            }
            catch
            {
            }
        }
    }
}
