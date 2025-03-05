using System;
using System.ComponentModel.Design;
using System.IO;
using EnvDTE80;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Engine.ReportGenerator;
using FineCodeCoverage.ReportGeneration;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using SharedProject.Core.CoverageToolOutput;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Output
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class OpenHotspotsCommand : IListener<ReportFilesMessage>, IListener<OutdatedOutputMessage>
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = PackageIds.cmdidOpenHotspotsCommand;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = PackageGuids.guidFCCPackageCmdSet;

        private readonly DTE2 dte;
        private readonly MenuCommand command;
        private IReportResult reportResult;
        private string hotspotsPath;
        private readonly IHotspotsService hotspotsService;

        public static OpenHotspotsCommand Instance
        {
            get;
            private set;
        }


        public static async Task InitializeAsync(AsyncPackage package, IEventAggregator eventAggregator, IHotspotsService hotspotsService)
        {
            // Switch to the main thread - the call to AddCommand in the constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            var dte = ServiceProvider.GlobalProvider.GetService(typeof(SDTE)) as DTE2;
            //var dte = package.GetServiceAsync(typeof(SDTE)) as DTE2;
            Instance = new OpenHotspotsCommand(commandService, eventAggregator, hotspotsService, dte);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClearUICommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private OpenHotspotsCommand(OleMenuCommandService commandService, IEventAggregator eventAggregator,IHotspotsService hotspotService, DTE2 dte)
        {
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            this.hotspotsService = hotspotService;
            var menuCommandID = new CommandID(CommandSet, CommandId);
            this.command = new MenuCommand(this.Execute, menuCommandID);
            command.Enabled = false;
            commandService.AddCommand(command);
            eventAggregator.AddListener(this);
            this.dte = dte;
        }

        public void Handle(ReportFilesMessage message)
        {
            reportResult = message.ReportResult;
            hotspotsPath = Path.Combine(Path.GetDirectoryName(message.CoberturaFile),"hotspots.xml");
            command.Enabled = true;
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
            if(reportResult != null)
            {

                // need the IHotspotService 
                hotspotsService.WriteHotspotsToXml(reportResult.Assemblies, hotspotsPath);
                dte.ItemOperations.OpenFile(hotspotsPath, EnvDTE.Constants.vsViewKindCode);
            }
        }

        public void Handle(OutdatedOutputMessage message)
        {
            command.Enabled = false;
        }
    }
}
