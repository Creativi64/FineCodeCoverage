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

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    [Export(typeof(ITUnitProjectFactory))]
    internal class TUnitProjectFactory : ITUnitProjectFactory
    {
        private readonly ITUnitInstalledPackagesService _tUnitInstalledPackagesService;
        private readonly ICommandLineParser _commandLineParser;

        class TUnitProject : ITUnitProject, IDisposable
        {
            private readonly ITUnitInstalledPackagesService _tUnitInstalledPackagesService;
            private readonly ICommandLineParser _commandLineParser;
            private IImmutableDictionary<string, IImmutableDictionary<string, string>> _packageReferenceItems;
            private bool _requiresUpdate = true;
            private bool _disposedValue;
            private readonly IProjectProperties _commonProperties;
            private readonly IDisposable _packageChangeSubscription;
            private const string FCCTestingPlatformCommandLineArgumentsPropertyName = "FCCTestingPlatformCommandLineArguments";
            private const string TestingPlatformCommandLineArgumentsPropertyName = "TestingPlatformCommandLineArguments";
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
                IVsHierarchy hierarchy
            )
            {
                this._commonProperties = configuredProject.Services.ProjectPropertiesProvider.GetCommonProperties();
                this.Hierarchy = hierarchy;
                this._tUnitInstalledPackagesService = tUnitInstalledPackagesService;
                this._commandLineParser = commandLineParser;
                this._packageChangeSubscription = this.SubscribeToPackageReferenceChanges(configuredProject);
            }

            /*
                cannot use GetEvaluatedPropertyValueAsync as absence returns empty string
            */
            private async Task<bool?> UseFCCTestingPlatformCommandLineArgumentsPropertyNameAsync()
            {
                System.Collections.Generic.IEnumerable<string> propertyNames = await this._commonProperties.GetPropertyNamesAsync();
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

                return hasTestingPlatformCommandLineArgumentsPropertyName ? false : (bool?)null;
            }

            private async Task ParseTestingPlatformCommandLineArgumentsAsync()
            {
                bool? useFCCTestingPlatformCommandLineArgumentsPropertyName = await this.UseFCCTestingPlatformCommandLineArgumentsPropertyNameAsync();
                if (!useFCCTestingPlatformCommandLineArgumentsPropertyName.HasValue)
                {
                    this.CommandLineParseResult = CommandLineParseResult.Empty;
                }
                else
                {
                    string propertyName = useFCCTestingPlatformCommandLineArgumentsPropertyName.Value ? FCCTestingPlatformCommandLineArgumentsPropertyName : TestingPlatformCommandLineArgumentsPropertyName;
                    string testingPlatformCommandLineArguments = await this._commonProperties.GetEvaluatedPropertyValueAsync(propertyName);

                    this.CommandLineParseResult = this._commandLineParser.Parse(testingPlatformCommandLineArguments);
                }
            }

            private IDisposable SubscribeToPackageReferenceChanges(ConfiguredProject configuredProject)
            {
                // there is ActiveConfiguredProjectSubscription but not available in 2019
                IProjectSubscriptionService subscriptionService = configuredProject.Services.ProjectSubscription;
                var receivingBlock = new ActionBlock<IProjectVersionedValue<IProjectSubscriptionUpdate>>(this.ProjectUpdateAsync);
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
                this._packageReferenceItems = update.Value.CurrentState["PackageReference"].Items;
                this._requiresUpdate = true;
                return Task.CompletedTask;
            }
            public bool IsTUnit { get; private set; }
            public bool HasCoverageExtension { get; private set; }
            public IVsHierarchy Hierarchy { get; }

            public CommandLineParseResult CommandLineParseResult { get; private set; } = CommandLineParseResult.Empty;

            public async Task UpdateStateAsync(CancellationToken cancellationToken)
            {
                if (this._requiresUpdate)
                {
                    TUnitInstalledPackageResult installedPackagesResult = await this._tUnitInstalledPackagesService.GetTUnitInstalledPackagesAsync(await this.Hierarchy.GetGuidAsync(), cancellationToken);
                    if (installedPackagesResult.Status != InstalledPackageResultStatus.Successful)
                    {
                        // fallback but not transitive
                        // the data flow block should get data immediately
                        installedPackagesResult = this._tUnitInstalledPackagesService.GetTUnitInstalledPackages(this._packageReferenceItems);
                    }

                    this.IsTUnit = installedPackagesResult.HasTUnit;
                    this.HasCoverageExtension = installedPackagesResult.HasCoverageExtension;

                    this._requiresUpdate = false;
                }

                if (!this.IsTUnit)
                {
                    return;
                }
                /*
                    alternative is 
                    var projectSnapshotService = configuredProject.Services.ProjectSnapshotService;
                    var receivingBlock = new ActionBlock<IProjectVersionedValue<IProjectSnapshot>>((pvv) =>
                    {
                        var projectInstance = pvv.Value.ProjectInstance;
                        var argsProperty = projectInstance.GetProperty(FCCTestingPlatformCommandLineArgumentsPropertyName);
                        if (argsProperty == null)
                        {
                            argsProperty = projectInstance.GetProperty(TestingPlatformCommandLineArgumentsPropertyName);
                        }
                        if(argsProperty != null)
                        {
                            var value = argsProperty.EvaluatedValue;
                        }

                    });
                    return projectSnapshotService.SourceBlock.LinkTo(receivingBlock);

                */
                await this.ParseTestingPlatformCommandLineArgumentsAsync();
            }

            protected virtual void Dispose(bool disposing)
            {
                if (this._disposedValue)
                {
                    return;
                }

                if (disposing)
                {
                    this._packageChangeSubscription.Dispose();
                }

                this._disposedValue = true;
            }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                this.Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

        [ImportingConstructor]
        public TUnitProjectFactory(
            ITUnitInstalledPackagesService tUnitInstalledPackagesService,
            ICommandLineParser commandLineParser
        )
        {
            this._tUnitInstalledPackagesService = tUnitInstalledPackagesService;
            this._commandLineParser = commandLineParser;
        }
        public ITUnitProject Create(IVsHierarchy hierarchy, ConfiguredProject configuredProject)
            => new TUnitProject(this._tUnitInstalledPackagesService, this._commandLineParser, configuredProject, hierarchy);
    }
}
