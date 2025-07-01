using System;

namespace FineCodeCoverage.Collection.CoverageToolOutput
{
    internal interface ICoverageToolOutputFolderSolutionProvider
    {
        string Provide(Func<string> solutionFolderProvider);
    }
}
