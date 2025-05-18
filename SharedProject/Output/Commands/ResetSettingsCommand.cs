using System;
using System.ComponentModel.Design;
using FineCodeCoverage.Options;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Output
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ResetSettingsCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = PackageIds.cmdidResetSettingsCommand;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = PackageGuids.guidFCCPackageCmdSet;

        private readonly MenuCommand command;
        private readonly ResetOptionsService resetOptionsService;

        public static ResetSettingsCommand Instance
        {
            get;
            private set;
        }

        public static async Task InitializeAsync(
            AsyncPackage package, ResetOptionsService resetOptionsService)
        {
            // Switch to the main thread - the call to AddCommand in ResetSettingsCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ResetSettingsCommand(commandService, resetOptionsService);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResetSettingsCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="commandService">Command service to add command to, not null.</param>
        /// <param name="resetOptionsService">Service that resets the options</param>
        private ResetSettingsCommand(OleMenuCommandService commandService, ResetOptionsService resetOptionsService)
        {
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            this.command = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(this.command);
            this.resetOptionsService = resetOptionsService;
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
            => this.resetOptionsService.Reset();
    }
}

