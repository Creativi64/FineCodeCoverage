using System;
using System.ComponentModel.Composition;
using System.IO;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Engine
{
    [Order(2, typeof(ICoverageToolOutputFolderSolutionProvider))]
    class FccOutputExistenceCoverageToolOutputFolderSolutionProvider : ICoverageToolOutputFolderSolutionProvider
    {
        private const string fccOutputFolderName = "fcc-output";
        private readonly IFileUtil _fileUtil;

        [ImportingConstructor]
        public FccOutputExistenceCoverageToolOutputFolderSolutionProvider(IFileUtil fileUtil)
            => this._fileUtil = fileUtil;

        public string Provide(Func<string> solutionFolderProvider)
        {
            string solutionFolder = solutionFolderProvider();
            if (solutionFolder != null)
            {
                string provided = Path.Combine(solutionFolder, fccOutputFolderName);
                if (this._fileUtil.DirectoryExists(provided))
                {
                    return provided;
                }
            }

            return null;
        }
    }
}