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
        private readonly ISolutionFolderProvider _solutionFolderProvider;
        private readonly IOrderedEnumerable<
            Lazy<ICoverageToolOutputFolderSolutionProvider, IOrderMetadata>
        > _solutionFolderProviders;

        [ImportingConstructor]
        public CoverageToolOutputFolderFromSolutionProvider(
            ISolutionFolderProvider solutionFolderProvider,
            [ImportMany]
            IEnumerable<Lazy<ICoverageToolOutputFolderSolutionProvider, IOrderMetadata>> solutionFolderProviders
        )
        {
            this._solutionFolderProvider = solutionFolderProvider;
            this._solutionFolderProviders = solutionFolderProviders.OrderBy(p => p.Metadata.Order);
        }

        public string Provide(List<ICoverageProject> coverageProjects)
        {
            bool provided = false;
            string providedDirectory = null;
            return this._solutionFolderProviders.SelectFirstNonNull(p => p.Value.Provide(() =>
            {
                if (!provided)
                {
                    providedDirectory = this._solutionFolderProvider.Provide(coverageProjects[0].ProjectFilePath);
                    provided = true;
                }

                return providedDirectory;
            }));
        }
    }
}