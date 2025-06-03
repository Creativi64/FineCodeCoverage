using System.ComponentModel.Composition;
using FineCodeCoverage.Engine.FileSynchronization;
using FineCodeCoverage.Options;

namespace FineCodeCoverage.Engine.Model
{
    [Export(typeof(ICoverageProjectFactory))]
    internal class CoverageProjectFactory : ICoverageProjectFactory
    {
        private readonly IOptionsProvider<OutputOptions> outputOptionsProvider;
        private readonly IFileSynchronizationUtil fileSynchronizationUtil;
        private readonly ICoverageProjectSettingsManager coverageProjectSettingsManager;
        private readonly IReferencedProjectsHelper referencedProjectsHelper;

        [ImportingConstructor]
        public CoverageProjectFactory(
            IOptionsProvider<OutputOptions> outputOptionsProvider,
            IFileSynchronizationUtil fileSynchronizationUtil,
            ICoverageProjectSettingsManager coverageProjectSettingsManager,
            IReferencedProjectsHelper referencedProjectsHelper)
        {
            this.outputOptionsProvider = outputOptionsProvider;
            this.fileSynchronizationUtil = fileSynchronizationUtil;
            this.coverageProjectSettingsManager = coverageProjectSettingsManager;
            this.referencedProjectsHelper = referencedProjectsHelper;
        }

        public ICoverageProject Create() => new CoverageProject(
                this.outputOptionsProvider,
                this.fileSynchronizationUtil,
                this.coverageProjectSettingsManager,
                this.referencedProjectsHelper);
    }
}