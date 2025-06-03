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
        private readonly ILogger logger;
        private readonly IEventAggregator eventAggregator;
        private readonly IFileUtil fileUtil;
        private string outputFolderForAllProjects;
        private List<ICoverageProject> coverageProjects;
        private readonly IOrderedEnumerable<Lazy<ICoverageToolOutputFolderProvider, IOrderMetadata>> outputFolderProviders;

        [ImportingConstructor]
        public CoverageToolOutputManager(
            IFileUtil fileUtil,
            ILogger logger, [ImportMany] IEnumerable<Lazy<ICoverageToolOutputFolderProvider, IOrderMetadata>> outputFolderProviders,
            IEventAggregator eventAggregator
            )
        {
            this.logger = logger;
            this.eventAggregator = eventAggregator;
            this.fileUtil = fileUtil;
            this.outputFolderProviders = outputFolderProviders.OrderBy(p => p.Metadata.Order);
        }

        public async Task SetProjectCoverageOutputFolderAsync(List<ICoverageProject> coverageProjects)
        {
            this.eventAggregator.SendMessage(new OutdatedOutputMessage());
            this.coverageProjects = coverageProjects;
            await this.DetermineOutputFolderForAllProjectsAsync();
            if (this.outputFolderForAllProjects == null)
            {
                foreach (ICoverageProject coverageProject in coverageProjects)
                {
                    coverageProject.CoverageOutputFolder = coverageProject.DefaultCoverageOutputFolder;
                }
            }
            else
            {
                this.fileUtil.TryEmptyDirectory(this.outputFolderForAllProjects);
                foreach (ICoverageProject coverageProject in coverageProjects)
                {
                    coverageProject.CoverageOutputFolder = Path.Combine(this.outputFolderForAllProjects, coverageProject.ProjectName);
                }
            }
        }

        private async Task DetermineOutputFolderForAllProjectsAsync()
        {
            this.outputFolderForAllProjects = this.outputFolderProviders.SelectFirstNonNull(p => p.Value.Provide(this.coverageProjects));
            if (this.outputFolderForAllProjects != null)
            {
                await this.logger.LogAsync($"FCC output in {this.outputFolderForAllProjects}");
            }
        }

        public string GetReportOutputFolder()
            => this.outputFolderForAllProjects ?? this.coverageProjects[0].CoverageOutputFolder;
    }
}