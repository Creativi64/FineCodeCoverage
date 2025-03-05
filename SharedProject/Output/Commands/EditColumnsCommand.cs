using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Output
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class EditColumnsCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = PackageIds.cmdidEditColumnsCommand;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = PackageGuids.guidFCCPackageCmdSet;
        private readonly IReportColumnsService reportColumnsService;


        /// <summary>
        /// Initializes a new instance of the <see cref="EditColumnsCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private EditColumnsCommand(OleMenuCommandService commandService, IReportColumnsService reportColumnsService)
        {
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
            this.reportColumnsService = reportColumnsService;
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static EditColumnsCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package, IReportColumnsService reportColumnsService)
        {
            // Switch to the main thread - the call to AddCommand in ClearUICommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new EditColumnsCommand(commandService, reportColumnsService);
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
            ThreadHelper.ThrowIfNotOnUIThread();
            reportColumnsService.ManageColumns();
        }

    }
}
