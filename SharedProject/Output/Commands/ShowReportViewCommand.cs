using System;
using System.ComponentModel.Design;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Output
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ShowReportViewCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = PackageIds.cmdidShowReportViewCommand;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = PackageGuids.guidFCCPackageCmdSet;

        private readonly MenuCommand command;
        private readonly IReportViewService reportViewService;

        public static ShowReportViewCommand Instance
        {
            get;
            private set;
        }


        public static async Task InitializeAsync(AsyncPackage package, IReportViewService reportViewService)
        {
            // Switch to the main thread - the call to AddCommand in the constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            var dte = ServiceProvider.GlobalProvider.GetService(typeof(SDTE)) as DTE2;
            Instance = new ShowReportViewCommand(commandService, reportViewService);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowReportViewCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private ShowReportViewCommand(OleMenuCommandService commandService, IReportViewService reportViewService)
        {
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            this.command = new MenuCommand(this.Execute, menuCommandID);
            this.command.Visible = false;
            commandService.AddCommand(command);
            this.reportViewService = reportViewService;
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            reportViewService.Show();
        }
    }
}
