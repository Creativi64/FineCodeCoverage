using System;
using System.ComponentModel.Design;
using FineCodeCoverage.Github;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Output
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class NewIssueCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = PackageIds.cmdidNewIssueCommand;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = PackageGuids.guidFCCPackageCmdSet;

        private readonly MenuCommand command;
        private readonly IFCCGithubService fccGithubService;

        public static NewIssueCommand Instance
        {
            get;
            private set;
        }

        public static async Task InitializeAsync(AsyncPackage package, IFCCGithubService fccGithubService)
        {
            // Switch to the main thread - the call to AddCommand in NewIssueCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new NewIssueCommand(commandService, fccGithubService);
        }

        private NewIssueCommand(OleMenuCommandService commandService, IFCCGithubService fccGithubService)
        {
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            this.command = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(this.command);
            this.fccGithubService = fccGithubService;
        }

        private void Execute(object sender, EventArgs e)
            => this.fccGithubService.NewIssue();
    }
}