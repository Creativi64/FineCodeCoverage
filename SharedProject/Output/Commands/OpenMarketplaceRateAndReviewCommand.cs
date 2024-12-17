using System;
using System.ComponentModel.Design;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Options;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Output
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class OpenMarketplaceRateAndReviewCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = PackageIds.cmdidMarketplaceRateAndReviewCommand;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = PackageGuids.guidFCCPackageCmdSet;

        private readonly MenuCommand command;
        private readonly IOpenFCCVsMarketplace openFCCVsMarketplace;

        public static OpenMarketplaceRateAndReviewCommand Instance
        {
            get;
            private set;
        }

        public static async Task InitializeAsync(AsyncPackage package,IOpenFCCVsMarketplace openFCCVsMarketplace)
        {
            // Switch to the main thread - the call to AddCommand in OpenMarketplaceRateAndReviewCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new OpenMarketplaceRateAndReviewCommand(commandService, openFCCVsMarketplace);
        }

        private OpenMarketplaceRateAndReviewCommand(OleMenuCommandService commandService, IOpenFCCVsMarketplace openFCCVsMarketplace)
        {
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            this.command = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(this.command);
            this.openFCCVsMarketplace = openFCCVsMarketplace;
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
            => this.openFCCVsMarketplace.OpenRatingAndReview();
    }
}

