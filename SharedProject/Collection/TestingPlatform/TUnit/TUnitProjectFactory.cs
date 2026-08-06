using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.Shell.Interop;
using NuGet.VisualStudio.Contracts;

namespace FineCodeCoverage.Collection.TestingPlatform.TUnit
{
    [Export(typeof(ITUnitProjectFactory))]
    internal sealed class TUnitProjectFactory : ITUnitProjectFactory
    {
        private readonly ITUnitInstalledPackagesService _tUnitInstalledPackagesService;
        private readonly ICommandLineParser _commandLineParser;

        private sealed class TUnitProject : ITUnitProject, IDisposable
        {
            private const string FCCTestingPlatformCommandLineArgumentsPropertyName = "FCCTestingPlatformCommandLineArguments";
            private const string TestingPlatformCommandLineArgumentsPropertyName = "TestingPlatformCommandLineArguments";
            private const string IsTestingPlatformApplicationPropertyName = "IsTestingPlatformApplication";
            private readonly ITUnitInstalledPackagesService _tUnitInstalledPackagesService;
            private readonly ICommandLineParser _commandLineParser;
            private readonly IProjectProperties _commonProperties;
            private readonly IDisposable _packageChangeSubscription;
            private readonly IDisposable _snapshotSubscription;
            private IImmutableDictionary<string, IImmutableDictionary<string, string>> _packageReferenceItems;
            private Microsoft.Build.Execution.ProjectInstance _projectInstance;
            private bool _requiresUpdate = true;
            private bool _disposedValue;

            private static readonly string[] s_packageReferenceRuleNames = new string[] { "PackageReference" };

            /*
                in VS2022 there is also
                https://learn.microsoft.com/en-us/visualstudio/extensibility/visualstudio.extensibility/project/project?view=vs-2022
                https://learn.microsoft.com/en-us/visualstudio/extensibility/project-visual-studio-sdk?view=vs-2022  o
            */

            public TUnitProject(
                ITUnitInstalledPackagesService tUnitInstalledPackagesService,
                ICommandLineParser commandLineParser,
                ConfiguredProject configuredProject,
                IVsHierarchy hierarchy)
            {
                _commonProperties = configuredProject.Services.ProjectPropertiesProvider.GetCommonProperties();
                Hierarchy = hierarchy;
                _tUnitInstalledPackagesService = tUnitInstalledPackagesService;
                _commandLineParser = commandLineParser;
                _packageChangeSubscription = SubscribeToPackageReferenceChanges(configuredProject);
                _snapshotSubscription = SubscribeToProjectSnapshotChanges(configuredProject);
            }

            /*
                cannot use GetEvaluatedPropertyValueAsync as absence returns empty string.

                GetCommonProperties() is backed by the project file itself, so its GetPropertyNamesAsync
                only enumerates properties authored in the .csproj - a property coming from an import
                such as Directory.Build.props is invisible to it.  The evaluated ProjectInstance from the
                project snapshot sees the whole evaluation, imports included, and GetProperty returns null
                for a property that was never defined, so it distinguishes absence from empty.
            */
            private async Task<bool?> UseFCCTestingPlatformCommandLineArgumentsPropertyNameAsync()
            {
                Microsoft.Build.Execution.ProjectInstance projectInstance = _projectInstance;
                if (projectInstance != null)
                {
                    if (projectInstance.GetProperty(FCCTestingPlatformCommandLineArgumentsPropertyName) != null)
                    {
                        return true;
                    }

                    return projectInstance.GetProperty(TestingPlatformCommandLineArgumentsPropertyName) != null ? false : null;
                }

                // the snapshot has not arrived yet - fall back to the project file properties
                System.Collections.Generic.IEnumerable<string> propertyNames = await _commonProperties.GetPropertyNamesAsync();
                bool hasTestingPlatformCommandLineArgumentsPropertyName = false;
                foreach (string propertyName in propertyNames)
                {
                    if (propertyName == FCCTestingPlatformCommandLineArgumentsPropertyName)
                    {
                        return true;
                    }

                    if (propertyName == TestingPlatformCommandLineArgumentsPropertyName)
                    {
                        hasTestingPlatformCommandLineArgumentsPropertyName = true;
                    }
                }

                return hasTestingPlatformCommandLineArgumentsPropertyName ? false : null;
            }

            private async Task<string> GetCommandLineArgumentsPropertyValueAsync(string propertyName)
            {
                Microsoft.Build.Execution.ProjectInstance projectInstance = _projectInstance;
                return projectInstance != null
                    ? projectInstance.GetProperty(propertyName)?.EvaluatedValue
                    : await _commonProperties.GetEvaluatedPropertyValueAsync(propertyName);
            }

            private async Task<bool> IsTestingPlatformApplicationAsync()
            {
                // absence evaluates to empty string; MTP SDKs set this to "true"
                string value = await _commonProperties.GetEvaluatedPropertyValueAsync(IsTestingPlatformApplicationPropertyName);
                return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
            }

            private async Task ParseTestingPlatformCommandLineArgumentsAsync()
            {
                bool? useFCCTestingPlatformCommandLineArgumentsPropertyName = await UseFCCTestingPlatformCommandLineArgumentsPropertyNameAsync();
                if (!useFCCTestingPlatformCommandLineArgumentsPropertyName.HasValue)
                {
                    CommandLineParseResult = CommandLineParseResult.Empty;
                }
                else
                {
                    string propertyName = useFCCTestingPlatformCommandLineArgumentsPropertyName.Value ? FCCTestingPlatformCommandLineArgumentsPropertyName : TestingPlatformCommandLineArgumentsPropertyName;
                    string testingPlatformCommandLineArguments = await GetCommandLineArgumentsPropertyValueAsync(propertyName);

                    CommandLineParseResult = _commandLineParser.Parse(testingPlatformCommandLineArguments);
                }
            }

            private IDisposable SubscribeToPackageReferenceChanges(ConfiguredProject configuredProject)
            {
                // there is ActiveConfiguredProjectSubscription but not available in 2019
                IProjectSubscriptionService subscriptionService = configuredProject.Services.ProjectSubscription;
                var receivingBlock = new ActionBlock<IProjectVersionedValue<IProjectSubscriptionUpdate>>(ProjectUpdateAsync);
                return subscriptionService.JointRuleSource.SourceBlock.LinkTo(receivingBlock, ruleNames: s_packageReferenceRuleNames);
            }

            /*
                Idea was to use Nuget api, but
                IVsPackageInstallerEvents
                These events are only raised for packages.config projects.
                To get updates for both packages.config and PackageReference use IVsNuGetProjectUpdateEvents instead.

                But IVsNuGetProjectUpdateEvents shipped in version 6.2 - Visual Studio 2022

                --
                Also note that IVSProject4 has PackageReferences but the project is IVSProject !
                and cannot get change event from VSProjectEvents.ReferencesEvents
            */

            /*
                if did not want real-time changes then could have used configuredProject.Services.PackageReferences
                public interface IPackageReference : IReference
                {
                }
            */

            private Task ProjectUpdateAsync(IProjectVersionedValue<IProjectSubscriptionUpdate> update)
            {
                // if need to switch to the main thread will need CPS IThreadHandling
                // This runs on a background thread.
                _packageReferenceItems = update.Value.CurrentState["PackageReference"].Items;
                _requiresUpdate = true;
                return Task.CompletedTask;
            }

            private IDisposable SubscribeToProjectSnapshotChanges(ConfiguredProject configuredProject)
            {
                var receivingBlock = new ActionBlock<IProjectVersionedValue<IProjectSnapshot>>(SnapshotUpdate);
                return configuredProject.Services.ProjectSnapshotService.SourceBlock.LinkTo(receivingBlock);
            }

            private void SnapshotUpdate(IProjectVersionedValue<IProjectSnapshot> snapshot)
                => _projectInstance = snapshot.Value.ProjectInstance;

            public bool IsTUnit { get; private set; }

            public bool HasCoverageExtension { get; private set; }

            public IVsHierarchy Hierarchy { get; }

            public CommandLineParseResult CommandLineParseResult { get; private set; } = CommandLineParseResult.Empty;

            public async Task UpdateStateAsync(CancellationToken cancellationToken)
            {
                if (_requiresUpdate)
                {
                    TUnitInstalledPackageResult installedPackagesResult = await _tUnitInstalledPackagesService.GetTUnitInstalledPackagesAsync(await Hierarchy.GetGuidAsync(), cancellationToken);
                    if (installedPackagesResult.Status != InstalledPackageResultStatus.Successful)
                    {
                        // fallback but not transitive
                        // the data flow block should get data immediately
                        installedPackagesResult = _tUnitInstalledPackagesService.GetTUnitInstalledPackages(_packageReferenceItems);
                    }

                    HasCoverageExtension = installedPackagesResult.HasCoverageExtension;

                    // IsTUnit now means "is a Microsoft.Testing.Platform test project we can collect coverage for".
                    // Package detection misses SDK-style MTP projects (e.g. MSTest.Sdk references the platform
                    // transitively, not as a PackageReference), so the reliable signal is the IsTestingPlatformApplication
                    // MSBuild property, which every MTP SDK (TUnit, MSTest.Sdk, xUnit v3, NUnit-MTP) sets to true.
                    IsTUnit = installedPackagesResult.HasTUnit || await IsTestingPlatformApplicationAsync();

                    _requiresUpdate = false;
                }

                if (!IsTUnit)
                {
                    return;
                }

                await ParseTestingPlatformCommandLineArgumentsAsync();
            }

            private void Dispose(bool disposing)
            {
                if (_disposedValue)
                {
                    return;
                }

                if (disposing)
                {
                    _packageChangeSubscription.Dispose();
                    _snapshotSubscription.Dispose();
                }

                _disposedValue = true;
            }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

        [ImportingConstructor]
        public TUnitProjectFactory(
            ITUnitInstalledPackagesService tUnitInstalledPackagesService,
            ICommandLineParser commandLineParser)
        {
            _tUnitInstalledPackagesService = tUnitInstalledPackagesService;
            _commandLineParser = commandLineParser;
        }

        public ITUnitProject Create(IVsHierarchy hierarchy, ConfiguredProject configuredProject)
            => new TUnitProject(_tUnitInstalledPackagesService, _commandLineParser, configuredProject, hierarchy);
    }
}
