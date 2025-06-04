using System;
using System.ComponentModel.Composition;
using System.IO;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Engine
{
    [Order(2, typeof(ICoverageToolOutputFolderSolutionProvider))]
    internal class FccOutputExistenceCoverageToolOutputFolderSolutionProvider : ICoverageToolOutputFolderSolutionProvider
    {
        private const string FCCOutputFolderName = "fcc-output";
        private readonly IFileUtil _fileUtil;

        [ImportingConstructor]
        public FccOutputExistenceCoverageToolOutputFolderSolutionProvider(IFileUtil fileUtil)
            => _fileUtil = fileUtil;

        public string Provide(Func<string> solutionFolderProvider)
        {
            string solutionFolder = solutionFolderProvider();
            if (solutionFolder == null)
            {
                return null;
            }

            string provided = Path.Combine(solutionFolder, FCCOutputFolderName);
            return _fileUtil.DirectoryExists(provided) ? provided : null;
        }
    }
}
