using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine;
using SharedProject.Core.CoverageToolOutput;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICommand))]
    internal sealed class OpenCoberturaCommand : CommandBase, IListener<ReportFilesMessage>, IListener<OutdatedOutputMessage>
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IVsOpenFile vsOpenFile;
        private readonly IFileUtil fileUtil;
        private string coberturaFile;

        protected override int CommandId { get; } = PackageIds.cmdidOpenCoberturaCommand;
        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public OpenCoberturaCommand(IEventAggregator eventAggregator, IVsOpenFile vsOpenFile, IFileUtil fileUtil)
        {
            this.eventAggregator = eventAggregator;
            this.vsOpenFile = vsOpenFile;
            this.fileUtil = fileUtil;
        }

        protected override void FinalInitialization()
        {
            this.Command.Enabled = false;
            this.eventAggregator.AddListener(this);
        }

        protected override void Execute(object sender, EventArgs e)
        {
            if (fileUtil.Exists(coberturaFile))
            {
                this.vsOpenFile.OpenFileInDefaultViewer(coberturaFile);
            }
        }

        public void Handle(OutdatedOutputMessage message)
        {
            Command.Enabled = false;
        }

        public void Handle(ReportFilesMessage message)
        {
            this.coberturaFile = message.CoberturaFile;
            Command.Enabled = true;
        }
    }
    /// <summary>
    /// Command handler
    /// </summary>
    //internal sealed class OpenCoberturaCommand : IListener<ReportFilesMessage>, IListener<OutdatedOutputMessage>
    //{
    //    /// <summary>
    //    /// Command ID.
    //    /// </summary>
    //    public const int CommandId = PackageIds.cmdidOpenCoberturaCommand;

    //    /// <summary>
    //    /// Command menu group (command set GUID).
    //    /// </summary>
    //    public static readonly Guid CommandSet = PackageGuids.guidFCCPackageCmdSet;

    //    private readonly DTE2 dte;
    //    private readonly MenuCommand command;
    //    private string coberturaFile;

    //    public static OpenCoberturaCommand Instance
    //    {
    //        get;
    //        private set;
    //    }


    //    public static async Task InitializeAsync(AsyncPackage package, IEventAggregator eventAggregator)
    //    {
    //        // Switch to the main thread - the call to AddCommand in the constructor requires
    //        // the UI thread.
    //        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

    //        OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
    //        var dte = ServiceProvider.GlobalProvider.GetService(typeof(SDTE)) as DTE2;
    //        Instance = new OpenCoberturaCommand(commandService, eventAggregator, dte);
    //    }

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="OpenCoberturaCommand"/> class.
    //    /// Adds our command handlers for menu (commands must exist in the command table file)
    //    /// </summary>
    //    /// <param name="commandService">Command service to add command to, not null.</param>
    //    private OpenCoberturaCommand(OleMenuCommandService commandService, IEventAggregator eventAggregator, DTE2 dte)
    //    {
    //        commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

    //        var menuCommandID = new CommandID(CommandSet, CommandId);
    //        this.command = new MenuCommand(this.Execute, menuCommandID);
    //        command.Enabled = false;
    //        commandService.AddCommand(command);
    //        eventAggregator.AddListener(this);
    //        this.dte = dte;
    //    }

    //    public void Handle(ReportFilesMessage message)
    //    {
    //        coberturaFile = message.CoberturaFile;
    //        command.Enabled = true;
    //    }



    //    /// <summary>
    //    /// This function is the callback used to execute the command when the menu item is clicked.
    //    /// See the constructor to see how the menu item is associated with this function using
    //    /// OleMenuCommandService service and MenuCommand class.
    //    /// </summary>
    //    /// <param name="sender">Event sender.</param>
    //    /// <param name="e">Event args.</param>
    //    private void Execute(object sender, EventArgs e)
    //    {
    //        ThreadHelper.ThrowIfNotOnUIThread();
    //        if (File.Exists(coberturaFile))
    //        {
    //            dte.ItemOperations.OpenFile(coberturaFile, EnvDTE.Constants.vsViewKindPrimary);
    //        }
    //    }

    //    public void Handle(OutdatedOutputMessage message)
    //    {
    //        command.Enabled = false;
    //    }
    //}
}
