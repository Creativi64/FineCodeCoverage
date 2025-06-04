using System.ComponentModel.Composition;
using FineCodeCoverage.Engine.FileSynchronization;
using FineCodeCoverage.Options;

namespace FineCodeCoverage.Engine.Model
{
    [Export(typeof(ICoverageProjectFactory))]
    internal class CoverageProjectFactory : ICoverageProjectFactory
    {
        private readonly IOptionsProvider<OutputOptions> _outputOptionsProvider;
        private readonly IFileSynchronizationUtil _fileSynchronizationUtil;
        private readonly ICoverageProjectSettingsManager _coverageProjectSettingsManager;
        private readonly IReferencedProjectsHelper _referencedProjectsHelper;

        [ImportingConstructor]
        public CoverageProjectFactory(
            IOptionsProvider<OutputOptions> outputOptionsProvider,
            IFileSynchronizationUtil fileSynchronizationUtil,
            ICoverageProjectSettingsManager coverageProjectSettingsManager,
            IReferencedProjectsHelper referencedProjectsHelper)
        {
            this._outputOptionsProvider = outputOptionsProvider;
            this._fileSynchronizationUtil = fileSynchronizationUtil;
            this._coverageProjectSettingsManager = coverageProjectSettingsManager;
            this._referencedProjectsHelper = referencedProjectsHelper;
        }

        public ICoverageProject Create()
            => new CoverageProject(
                this._outputOptionsProvider,
                this._fileSynchronizationUtil,
                this._coverageProjectSettingsManager,
                this._referencedProjectsHelper);
    }
}