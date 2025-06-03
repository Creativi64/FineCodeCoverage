using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine
{
    [Order(1, typeof(ICoverageToolOutputFolderProvider))]
    class CoverageToolOutputFolderFromSolutionProvider : ICoverageToolOutputFolderProvider
    {
        private readonly ISolutionFolderProvider solutionFolderProvider;
        private readonly IOrderedEnumerable<Lazy<ICoverageToolOutputFolderSolutionProvider, IOrderMetadata>> solutionFolderProviders;

        [ImportingConstructor]
        public CoverageToolOutputFolderFromSolutionProvider(ISolutionFolderProvider solutionFolderProvider, [ImportMany] IEnumerable<Lazy<ICoverageToolOutputFolderSolutionProvider, IOrderMetadata>> solutionFolderProviders)
        {
            this.solutionFolderProvider = solutionFolderProvider;
            this.solutionFolderProviders = solutionFolderProviders.OrderBy(p => p.Metadata.Order);
        }

        public string Provide(List<ICoverageProject> coverageProjects)
        {
            bool provided = false;
            string providedDirectory = null;
            return this.solutionFolderProviders.SelectFirstNonNull(p => p.Value.Provide(() =>
            {
                if (!provided)
                {
                    providedDirectory = this.solutionFolderProvider.Provide(coverageProjects[0].ProjectFilePath);
                    provided = true;
                }

                return providedDirectory;
            }));
        }
    }
}