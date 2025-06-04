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
            this._logger = logger;
            this._eventAggregator = eventAggregator;
            this._fileUtil = fileUtil;
            this._outputFolderProviders = outputFolderProviders.OrderBy(p => p.Metadata.Order);
        }

        public async Task SetProjectCoverageOutputFolderAsync(List<ICoverageProject> coverageProjects)
        {
            this._eventAggregator.SendMessage(new OutdatedOutputMessage());
            this._coverageProjects = coverageProjects;
            await this.DetermineOutputFolderForAllProjectsAsync();
            if (this._outputFolderForAllProjects == null)
            {
                foreach (ICoverageProject coverageProject in coverageProjects)
                {
                    coverageProject.CoverageOutputFolder = coverageProject.DefaultCoverageOutputFolder;
                }
            }
            else
            {
                this._fileUtil.TryEmptyDirectory(this._outputFolderForAllProjects);
                foreach (ICoverageProject coverageProject in coverageProjects)
                {
                    coverageProject.CoverageOutputFolder = Path.Combine(this._outputFolderForAllProjects, coverageProject.ProjectName);
                }
            }
        }

        private async Task DetermineOutputFolderForAllProjectsAsync()
        {
            this._outputFolderForAllProjects = this._outputFolderProviders.SelectFirstNonNull(p => p.Value.Provide(this._coverageProjects));
            if (this._outputFolderForAllProjects == null)
            {
                return;
            }

            await this._logger.LogAsync($"FCC output in {this._outputFolderForAllProjects}");
        }

        public string GetReportOutputFolder()
            => this._outputFolderForAllProjects ?? this._coverageProjects[0].CoverageOutputFolder;
    }
}
