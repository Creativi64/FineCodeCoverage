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
        private readonly IOptionsProvider<OutputOptions> _outputOptionsProvider;

        [ImportingConstructor]
        public AppOptionsCoverageToolOutputFolderSolutionProvider(
            IOptionsProvider<OutputOptions> outputOptionsProvider
        ) => this._outputOptionsProvider = outputOptionsProvider;

        public string Provide(Func<string> solutionFolderProvider)
        {
            OutputOptions appOptions = this._outputOptionsProvider.Get();
            if (string.IsNullOrEmpty(appOptions.FCCSolutionOutputDirectoryName))
            {
                return null;
            }

            string solutionFolder = solutionFolderProvider();
            return solutionFolder != null ? Path.Combine(solutionFolder, appOptions.FCCSolutionOutputDirectoryName) : null;
        }
    }
}
