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
        private readonly IShownToolWindowHistory shownToolWindowHistory;

        protected override int CommandId { get; } = PackageIds.cmdidOpenReportWindowCommand;
        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public OpenReportWindowCommand(IShownToolWindowHistory shownToolWindowHistory) => this.shownToolWindowHistory = shownToolWindowHistory;

        protected override void Execute(object sender, EventArgs e) => this.PackageServices.RunAsyncWithExceptionLogging(this.ShowToolWindowAsync);

        public async Task<ToolWindowPane> ShowToolWindowAsync()
        {
            this.shownToolWindowHistory.ShowedToolWindow();
            ToolWindowPane window = await this.PackageServices.ShowToolWindowAsync(typeof(ReportToolWindow), 0, true, this.PackageServices.DisposalToken);

            return this.ReturnOrThrowIfCannotCreateToolWindow(window);
        }

        private ToolWindowPane ReturnOrThrowIfCannotCreateToolWindow(ToolWindowPane window)
        {
            if ((window == null) || (window.Frame == null))
            {
                throw new NotSupportedException($"Cannot create '{Vsix.Name}' report window");
            }

            return window;
        }

        async Task IReportToolWindowOpener.TryOpenAsync()
        {
            try
            {
                await this.ShowToolWindowAsync();
            }
            catch { }
        }
    }
}
