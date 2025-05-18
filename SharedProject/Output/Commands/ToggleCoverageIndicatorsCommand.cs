using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICommand))]
    internal sealed class ToggleCoverageIndicatorsCommand : CommandBase
    {
        private readonly IEventAggregator eventAggregator;

        protected override int CommandId { get; } = PackageIds.cmdidToggleCoverageIndicatorsCommand;
        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public ToggleCoverageIndicatorsCommand(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        protected override void Execute(object sender, EventArgs e) => eventAggregator.SendMessage(new ToggleCoverageIndicatorsMessage());
    }
    /// <summary>
    /// Command handler
    /// </summary>
    //internal sealed class ToggleCoverageIndicatorsCommand
    //{
    //    /// <summary>
    //    /// Command ID.
    //    /// </summary>
    //    public const int CommandId = PackageIds.cmdidToggleCoverageIndicatorsCommand;

    //    /// <summary>
    //    /// Command menu group (command set GUID).
    //    /// </summary>
    //    public static readonly Guid CommandSet = PackageGuids.guidFCCPackageCmdSet;
    //    private readonly IEventAggregator eventAggregator;

    //    private ToggleCoverageIndicatorsCommand(OleMenuCommandService commandService, IEventAggregator eventAggregator)
    //    {
    //        commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
    //        var menuCommandID = new CommandID(CommandSet, CommandId);
    //        var menuItem = new MenuCommand(this.Execute, menuCommandID);
    //        commandService.AddCommand(menuItem);
    //        this.eventAggregator = eventAggregator;
    //    }

    //    /// <summary>
    //    /// Gets the instance of the command.
    //    /// </summary>
    //    public static ToggleCoverageIndicatorsCommand Instance
    //    {
    //        get;
    //        private set;
    //    }

    //    /// <summary>
    //    /// Initializes the singleton instance of the command.
    //    /// </summary>
    //    /// <param name="package">Owner package, not null.</param>
    //    public static async Task InitializeAsync(AsyncPackage package, IEventAggregator eventAggregator)
    //    {
    //        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

    //        OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
    //        Instance = new ToggleCoverageIndicatorsCommand(commandService, eventAggregator);
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
    //        eventAggregator.SendMessage(new ToggleCoverageIndicatorsMessage());
    //    }

    //}
}
