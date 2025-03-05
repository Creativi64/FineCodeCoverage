using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using FineCodeCoverage.Core.Utilities;
using Microsoft.VisualStudio.Threading;

namespace FineCodeCoverage.Output
{
	/// <summary>
	/// Command handler
	/// </summary>
	internal sealed class OpenReportWindowCommand
	{
		/// <summary>
		/// Command ID.
		/// </summary>
		public const int CommandId = PackageIds.cmdidOpenReportWindowCommand;

		/// <summary>
		/// Command menu group (command set GUID).
		/// </summary>
		public static readonly Guid CommandSet = PackageGuids.guidFCCPackageCmdSet;

		/// <summary>
		/// VS Package that provides this command, not null.
		/// </summary>
		private readonly AsyncPackage package;

        private readonly ILogger logger;
        private readonly IShownToolWindowHistory shownToolWindowHistory;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenReportWindowCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private OpenReportWindowCommand(AsyncPackage package, OleMenuCommandService commandService, ILogger logger, IShownToolWindowHistory shownToolWindowHistory)
		{
			this.logger = logger;
            this.shownToolWindowHistory = shownToolWindowHistory;
            this.package = package ?? throw new ArgumentNullException(nameof(package));
			commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

			var menuCommandID = new CommandID(CommandSet, CommandId);
			var menuItem = new MenuCommand(this.Execute, menuCommandID);
			commandService.AddCommand(menuItem);
		}

		/// <summary>
		/// Gets the instance of the command.
		/// </summary>
		public static OpenReportWindowCommand Instance
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the service provider from the owner package.
		/// </summary>
		public IAsyncServiceProvider ServiceProvider
		{
			get
			{
				return package;
			}
		}

		/// <summary>
		/// Initializes the singleton instance of the command.
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		public static async Task InitializeAsync(AsyncPackage package, ILogger logger, IShownToolWindowHistory shownToolWindowHistory)
		{
			// Switch to the main thread - the call to AddCommand in the constructor requires
			// the UI thread.
			await package.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

			OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
			Instance = new OpenReportWindowCommand(package, commandService, logger, shownToolWindowHistory);
		}

		/// <summary>
		/// Shows the tool window when the menu item is clicked.
		/// </summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event args.</param>
		public void Execute(object sender, EventArgs e)
		{
			package.JoinableTaskFactory.RunAsync(async () =>
			{
                try
                {
                    await ShowToolWindowAsync();
                }
                catch (Exception exception)
                {
                    logger.Log(exception);
                }
            }).Task.Forget();

        }

		public async Task<ToolWindowPane> ShowToolWindowAsync()
		{
			shownToolWindowHistory.ShowedToolWindow();
            ToolWindowPane window = await package.ShowToolWindowAsync(typeof(ReportToolWindow), 0, true, package.DisposalToken);

			return ReturnOrThrowIfCannotCreateToolWindow(window);
		}

		public async Task<ToolWindowPane> FindToolWindowAsync()
		{
            ToolWindowPane window = await package.FindToolWindowAsync(typeof(ReportToolWindow), 0, true, package.DisposalToken);

            return ReturnOrThrowIfCannotCreateToolWindow(window);
		}

		private ToolWindowPane ReturnOrThrowIfCannotCreateToolWindow(ToolWindowPane window)
        {
			if ((window == null) || (window.Frame == null))
			{
				throw new NotSupportedException($"Cannot create '{Vsix.Name}' output window");
			}

			return window;
		}
	}
}
