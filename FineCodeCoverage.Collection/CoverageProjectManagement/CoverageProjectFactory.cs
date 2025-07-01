using System.ComponentModel.Composition;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Engine.FileSynchronization;
using FineCodeCoverage.Options;

namespace FineCodeCoverage.Engine.Model
{
    [Export(typeof(ICoverageProjectFactory))]
    internal sealed class CoverageProjectFactory : ICoverageProjectFactory
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
            IReferencedProjectsHelper referencedProjectsHelper
        )
        {
            _outputOptionsProvider = outputOptionsProvider;
            _fileSynchronizationUtil = fileSynchronizationUtil;
            _coverageProjectSettingsManager = coverageProjectSettingsManager;
            _referencedProjectsHelper = referencedProjectsHelper;
        }

        public ICoverageProject Create()
            => new CoverageProject(
                _outputOptionsProvider,
                _fileSynchronizationUtil,
                _coverageProjectSettingsManager,
                _referencedProjectsHelper);
    }
}
