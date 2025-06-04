using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Initialization;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Core.Utilities.Solution;
using FineCodeCoverage.Options;
using FineCodeCoverage.Readme;
using Microsoft;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

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
    [TraceImageLibraryRegistration(TraceLevel.Off)]
    [Guid(PackageGuids.guidFCCPackageString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Id)]
    [ProvideOptionPage(typeof(IncludesExcludesOptionsPage), Vsix.Name, "Includes Excludes", 0, 0, true)]
    [ProvideOptionPage(typeof(EditorCoverageColouringOptionsPage), Vsix.Name, "Editor Colouring", 0, 0, true)]
    [ProvideOptionPage(typeof(HotspotThresholdsOptionsPage), Vsix.Name, "Hotspot Thresholds", 0, 0, true)]
    [ProvideOptionPage(typeof(MiscOptionsPage), Vsix.Name, "Misc", 0, 0, true)]
    [ProvideOptionPage(typeof(OutputOptionsPage), Vsix.Name, "Output", 0, 0, true)]
    [ProvideOptionPage(typeof(ReportOptionsPage), Vsix.Name, "Report", 0, 0, true)]
    [ProvideOptionPage(typeof(RunOptionsPage), Vsix.Name, "Run", 0, 0, true)]
    [ProvideOptionPage(typeof(CoverletOptionsPage), Vsix.Name, "Coverlet", 0, 0, true)]
    [ProvideOptionPage(typeof(OpenCoverOptionsPage), Vsix.Name, "OpenCover", 0, 0, true)]
    [ProvideProfile(typeof(ProfileManager), Vsix.Name, Vsix.Name, 101, 102, false)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideToolWindow(typeof(ReportToolWindow), Style = VsDockStyle.Tabbed, DockedHeight = 300, Window = EnvDTE.Constants.vsWindowKindOutput)]
    [ProvideToolWindow(typeof(ReadmeToolWindow), Orientation = ToolWindowOrientation.Right, Style = VsDockStyle.Tabbed, Width = 600, Height = 700)]
    [ProvideAppCommandLine(ClearSettingsOnShutdown.ClearSettingsOnShutdownOption, typeof(FCCPackage), Arguments = "0")]
    public sealed class FCCPackage : AsyncPackage
    {
        private ISolutionOptions _solutionOptions;

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
            IComponentModel componentModel = GetComponentModel();
            InstantiateAllDialogPages();
            await InitializeSolutionOptionsAsync(componentModel);
            ReflectionMEFToolWindowContextProvider.ComponentModel = componentModel;
            await InitializeCommandsAsync(componentModel);
            // note that exporting the package does not work
            componentModel.GetService<IToolWindowServiceInit>().Package = this;
            await componentModel.GetService<IInitializer>().InitializeAsync(cancellationToken);
        }

        private void InstantiateAllDialogPages()
            => typeof(FCCPackage).GetCustomAttributes<ProvideOptionPageAttribute>()
            .Select(a => a.PageType).ToList().ForEach(t => _ = GetDialogPage(t));

        private static IComponentModel GetComponentModel() => (IComponentModel)GetGlobalService(typeof(SComponentModel));

        private async Task InitializeSolutionOptionsAsync(IComponentModel componentModel)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(DisposalToken);
            _solutionOptions = componentModel.GetService<ISolutionOptions>();
            System.Collections.Generic.IEnumerable<string> keys = await _solutionOptions.GetKeysAsync();
            foreach (string key in keys)
            {
                AddOptionKey(key);
            }

            var solutionPersistence = await GetServiceAsync(typeof(SVsSolutionPersistence)) as IVsSolutionPersistence;
            Assumes.Present(solutionPersistence);
            foreach (string key in keys)
            {
                _ = solutionPersistence.LoadPackageUserOpts(this, key);
            }
        }

        protected override void OnLoadOptions(string key, Stream stream) => _solutionOptions.LoadOptions(key, stream);

        protected override void OnSaveOptions(string key, Stream stream) => _solutionOptions.SaveOptions(key, stream);

        private async Task InitializeCommandsAsync(IComponentModel componentModel)
        {
            ICommandPackageServices commandPackageServices = await CommandPackageServices.CreateAsync(this, componentModel.GetService<ILogger>());
            foreach (ICommandInitializer command in componentModel.GetExtensions<ICommandInitializer>())
            {
                await command.InitializeAsync(commandPackageServices);
            }
        }

        protected override Task<object> InitializeToolWindowAsync(
            Type toolWindowType,
            int id,
            CancellationToken cancellationToken
        ) => Task.FromResult(ReflectionMEFToolWindowContextProvider.GetToolWindowContext(toolWindowType));

        public override IVsAsyncToolWindowFactory GetAsyncToolWindowFactory(Guid toolWindowType)
        {
            bool isToolWindowWithContext = ReflectionMEFToolWindowContextProvider.IsToolWindowWithContext(GetType(), toolWindowType);
            return isToolWindowWithContext ? this : null;
        }

        protected override string GetToolWindowTitle(Type toolWindowType, int id)
            => toolWindowType == typeof(ReportToolWindow) ?
                $"{Vsix.Name} loading" : base.GetToolWindowTitle(toolWindowType, id);
    }
}
