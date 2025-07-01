using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Utilities.Extensions;
using FineCodeCoverage.Utilities.MEF;

namespace FineCodeCoverage.Collection.CoverageToolOutput
{
    [Order(1, typeof(ICoverageToolOutputFolderProvider))]
    internal sealed class CoverageToolOutputFolderFromSolutionProvider : ICoverageToolOutputFolderProvider
    {
        private readonly ISolutionFolderProvider _solutionFolderProvider;
        private readonly IOrderedEnumerable<
            Lazy<ICoverageToolOutputFolderSolutionProvider, IOrderMetadata>
        > _solutionFolderProviders;

        [ImportingConstructor]
        public CoverageToolOutputFolderFromSolutionProvider(
            ISolutionFolderProvider solutionFolderProvider,
            [ImportMany]
            IEnumerable<Lazy<ICoverageToolOutputFolderSolutionProvider, IOrderMetadata>> solutionFolderProviders)
        {
            _solutionFolderProvider = solutionFolderProvider;
            _solutionFolderProviders = solutionFolderProviders.OrderBy(p => p.Metadata.Order);
        }

        public string Provide(List<ICoverageProject> coverageProjects)
        {
            bool provided = false;
            string providedDirectory = null;
            return _solutionFolderProviders.SelectFirstNonNull(p => p.Value.Provide(() =>
            {
                if (!provided)
                {
                    providedDirectory = _solutionFolderProvider.Provide(coverageProjects[0].ProjectFilePath);
                    provided = true;
                }

                return providedDirectory;
            }));
        }
    }
}
