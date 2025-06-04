using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Engine
{
    [Export(typeof(ICoverageToolOutputManager))]
    internal class CoverageToolOutputManager : ICoverageToolOutputManager
    {
        private readonly ILogger _logger;
        private readonly IEventAggregator _eventAggregator;
        private readonly IFileUtil _fileUtil;
        private string _outputFolderForAllProjects;
        private List<ICoverageProject> _coverageProjects;
        private readonly IOrderedEnumerable<
            Lazy<ICoverageToolOutputFolderProvider, IOrderMetadata>
        > _outputFolderProviders;

        [ImportingConstructor]
        public CoverageToolOutputManager(
            IFileUtil fileUtil,
            ILogger logger,
            [ImportMany]
            IEnumerable<Lazy<ICoverageToolOutputFolderProvider, IOrderMetadata>> outputFolderProviders,
            IEventAggregator eventAggregator
            )
        {
            _logger = logger;
            _eventAggregator = eventAggregator;
            _fileUtil = fileUtil;
            _outputFolderProviders = outputFolderProviders.OrderBy(p => p.Metadata.Order);
        }

        public async Task SetProjectCoverageOutputFolderAsync(List<ICoverageProject> coverageProjects)
        {
            _eventAggregator.SendMessage(new OutdatedOutputMessage());
            _coverageProjects = coverageProjects;
            await DetermineOutputFolderForAllProjectsAsync();
            if (_outputFolderForAllProjects == null)
            {
                foreach (ICoverageProject coverageProject in coverageProjects)
                {
                    coverageProject.CoverageOutputFolder = coverageProject.DefaultCoverageOutputFolder;
                }
            }
            else
            {
                _fileUtil.TryEmptyDirectory(_outputFolderForAllProjects);
                foreach (ICoverageProject coverageProject in coverageProjects)
                {
                    coverageProject.CoverageOutputFolder = Path.Combine(_outputFolderForAllProjects, coverageProject.ProjectName);
                }
            }
        }

        private async Task DetermineOutputFolderForAllProjectsAsync()
        {
            _outputFolderForAllProjects = _outputFolderProviders.SelectFirstNonNull(p => p.Value.Provide(_coverageProjects));
            if (_outputFolderForAllProjects == null)
            {
                return;
            }

            await _logger.LogAsync($"FCC output in {_outputFolderForAllProjects}");
        }

        public string GetReportOutputFolder()
            => _outputFolderForAllProjects ?? _coverageProjects[0].CoverageOutputFolder;
    }
}
