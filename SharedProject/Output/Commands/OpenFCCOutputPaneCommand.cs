using System;
using System.ComponentModel.Design;
using FineCodeCoverage.Impl;
using FineCodeCoverage.Output.Pane;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Output
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class OpenFCCOutputPaneCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = PackageIds.cmdidOpenFCCOutputPaneCommand;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = PackageGuids.guidFCCPackageCmdSet;

        private readonly MenuCommand command;
        private readonly IShowFCCOutputPane showFCCOutputPane;

        public static OpenFCCOutputPaneCommand Instance
        {
            get;
            private set;
        }

        public static async Task InitializeAsync(AsyncPackage package, IShowFCCOutputPane showFCCOutputPane)
        {
            // Switch to the main thread - the call to AddCommand in OpenFCCOutputPaneCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new OpenFCCOutputPaneCommand(commandService, showFCCOutputPane);
        }

        private OpenFCCOutputPaneCommand(OleMenuCommandService commandService, IShowFCCOutputPane showFCCOutputPane)
        {
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            this.command = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(this.command);
            this.showFCCOutputPane = showFCCOutputPane;
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e) 
            => _ = this.showFCCOutputPane.ShowAsync();
    }
}

