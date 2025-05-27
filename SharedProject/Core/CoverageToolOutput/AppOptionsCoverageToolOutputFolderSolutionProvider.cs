using System;
using System.ComponentModel.Composition;
using System.IO;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Options;

namespace FineCodeCoverage.Engine
{
    [Order(1, typeof(ICoverageToolOutputFolderSolutionProvider))]
    class AppOptionsCoverageToolOutputFolderSolutionProvider : ICoverageToolOutputFolderSolutionProvider
    {
        private readonly IOptionsProvider<OutputOptions> outputOptionsProvider;

        [ImportingConstructor]
        public AppOptionsCoverageToolOutputFolderSolutionProvider(
            IOptionsProvider<OutputOptions> outputOptionsProvider
        )
        {
            this.outputOptionsProvider = outputOptionsProvider;
        }

        public string Provide(Func<string> solutionFolderProvider)
        {
            var appOptions = outputOptionsProvider.Get();
            if (!String.IsNullOrEmpty(appOptions.FCCSolutionOutputDirectoryName))
            {
                var solutionFolder = solutionFolderProvider();
                if (solutionFolder != null)
                {
                    return Path.Combine(solutionFolder, appOptions.FCCSolutionOutputDirectoryName);
                }
            }
            return null;
        }
    }
}
