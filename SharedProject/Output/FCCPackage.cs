using System;
using System.Threading;
using FineCodeCoverage.Options;
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.Shell.Interop;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Core.Initialization;
using FineCodeCoverage.ReportGeneration;
using FineCodeCoverage.Funding;
using FineCodeCoverage.Github;
using FineCodeCoverage.Readme;
using FineCodeCoverage.Output.Pane;
using Microsoft.VisualStudio.ComponentModelHost;
using FineCodeCoverage.Core.MsTestPlatform.TestingPlatform;
using System.IO;
using FineCodeCoverage.Core.Utilities.Solution;
using Microsoft;
using System.Linq;

namespace FineCodeCoverage.Output
{

    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [ProvideBindingPath]
    [TraceImageLibraryRegistration()]
	[Guid(PackageGuids.guidFCCPackageString)]
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Id)]
	[ProvideOptionPage(typeof(AppOptionsPage), Vsix.Name, "General", 0, 0, true)]
    [ProvideOptionPage(typeof(EditorCoverageColouringOptionsPage), Vsix.Name, "Editor Colouring", 0, 0, true)]
    [ProvideOptionPage(typeof(HotspotThresholdsOptionsPage), Vsix.Name, "Hotspot Thresholds", 0, 0, true)]
    [ProvideOptionPage(typeof(ToolsOptionsPage), Vsix.Name, "Tools", 0, 0, true)]
    [ProvideOptionPage(typeof(OutputOptionsPage), Vsix.Name, "Output", 0, 0, true)]
    [ProvideOptionPage(typeof(ReportOptionsPage), Vsix.Name, "Report", 0, 0, true)]
    [ProvideOptionPage(typeof(RunOptionsPage), Vsix.Name, "Run", 0, 0, true)]
    [ProvideProfile(typeof(ProfileManager), Vsix.Name, Vsix.Name, 101, 102, false)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[ProvideToolWindow(typeof(ReportToolWindow), Style = VsDockStyle.Tabbed, DockedHeight = 300, Window = EnvDTE.Constants.vsWindowKindOutput)]
    [ProvideToolWindow(typeof(ReadmeToolWindow), Orientation = ToolWindowOrientation.Right, Style = VsDockStyle.Tabbed, Width = 600, Height = 700)]
    [ProvideAppCommandLine(ClearSettingsOnShutdown.ClearSettingsOnShutdownOption,  typeof(FCCPackage), Arguments = "0")]
    public sealed class FCCPackage : AsyncPackage, IDialogPageInstantiator
    {
        private ISolutionOptions solutionOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="FCCPackage"/> class.
        /// </summary>
        public FCCPackage()
		{
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        /*
			Hack necessary for debugging in 2022 !
			https://developercommunity.visualstudio.com/t/vsix-tool-window-vs2022-different-instantiation-wh/1663280
		*/


        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            var componentModel = GetComponentModel();
            await InitializeSolutionOptionsAsync(componentModel);
            ReflectionMEFToolWindowContextProvider.ComponentModel = componentModel;
            var requireDialogPageInstantiators = componentModel.GetExtensions<IRequireDialogPageInstantiator>().ToList();
            foreach (var requireDialogPageInstantiator in requireDialogPageInstantiators)
            {
                requireDialogPageInstantiator.DialogPageInstantiator = this;
            }
            await InitializeCommandsAsync(componentModel);
            // note that exporting the package does not work
            componentModel.GetService<IToolWindowServiceInit>().Package = this;
            await componentModel.GetService<IInitializer>().InitializeAsync(cancellationToken);
        }

        void IDialogPageInstantiator.Instantiate<TOptions>()
        {
            var optionPageType = OptionsPageTypeLookup.GetOptionPageType<TOptions>();
            GetDialogPage(optionPageType);
        }

        private IComponentModel GetComponentModel()
        {
            return (IComponentModel)GetGlobalService(typeof(SComponentModel));
        }

        private async Task InitializeSolutionOptionsAsync(IComponentModel componentModel)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(DisposalToken);
            this.solutionOptions = componentModel.GetService<ISolutionOptions>();
            var keys = await solutionOptions.GetKeysAsync();
            foreach (var key in keys)
            {
                AddOptionKey(key);
            }
            var solutionPersistence = await this.GetServiceAsync(typeof(SVsSolutionPersistence)) as IVsSolutionPersistence;
            Assumes.Present(solutionPersistence);
            foreach (var key in keys)
            {
                solutionPersistence.LoadPackageUserOpts(this, key);
            }
        }

        protected override void OnLoadOptions(string key, Stream stream)
        {
            solutionOptions.LoadOptions(key, stream);
        }

        protected override void OnSaveOptions(string key, Stream stream)
        {
            solutionOptions.SaveOptions(key, stream);
        }


        private async Task InitializeCommandsAsync (IComponentModel componentModel)
		{
            var fccEngine = componentModel.GetService<IFCCEngine>();
            var eventAggregator = componentModel.GetService<IEventAggregator>();
			var hotspotService = componentModel.GetService<IHotspotsService>();
            await OpenCoberturaCommand.InitializeAsync(this, eventAggregator);
            await OpenHotspotsCommand.InitializeAsync(this, eventAggregator, hotspotService);
            await ClearUICommand.InitializeAsync(this, componentModel.GetService<IUIClearer>());
            await ToggleCoverageIndicatorsCommand.InitializeAsync(this, eventAggregator);
            await OpenReportWindowCommand.InitializeAsync(
                this,
                componentModel.GetService<ILogger>(),
                componentModel.GetService<IShownToolWindowHistory>()
            );
            await OpenFCCOutputPaneCommand.InitializeAsync(this, componentModel.GetService<IShowFCCOutputPane>());
            await OpenSettingsCommand.InitializeAsync(this);
            await OpenMarketplaceRateAndReviewCommand.InitializeAsync(this, componentModel.GetService<IOpenFCCVsMarketplace>());
            await OpenFCCGithubCommand.InitializeAsync(this, componentModel.GetService<IFCCGithubService>());
            await NewIssueCommand.InitializeAsync(this, componentModel.GetService<IFCCGithubService>());
            await OpenReadMeCommand.InitializeAsync(this, componentModel.GetService<IReadMeService>());
            await OpenFundingCommand.InitializeAsync(this, componentModel.GetService<IFundingService>());
            await EditColumnsCommand.InitializeAsync(this, componentModel.GetService<IReportColumnsService>());
            await CollectTUnitCommand.InitializeAsync(this, componentModel.GetService<ITUnitCoverage>());
            await CancelCollectTUnitCommand.InitializeAsync(this, componentModel.GetService<ITUnitCoverage>());
            await ShowReportViewCommand.InitializeAsync(this, componentModel.GetService<IReportViewService>());
        }

        protected override Task<object> InitializeToolWindowAsync(Type toolWindowType, int id, CancellationToken cancellationToken)
        {
			return Task.FromResult(ReflectionMEFToolWindowContextProvider.GetToolWindowContext(toolWindowType));
		}
        public override IVsAsyncToolWindowFactory GetAsyncToolWindowFactory(Guid toolWindowType)
		{
            var isToolWindowWithContext = ReflectionMEFToolWindowContextProvider.IsToolWindowWithContext(this.GetType(), toolWindowType);
            return isToolWindowWithContext ? this : null;
		}

		protected override string GetToolWindowTitle(Type toolWindowType, int id)
		{
			if (toolWindowType == typeof(ReportToolWindow))
			{
				return $"{Vsix.Name} loading";
			}

			return base.GetToolWindowTitle(toolWindowType, id);
		}
	}
}
