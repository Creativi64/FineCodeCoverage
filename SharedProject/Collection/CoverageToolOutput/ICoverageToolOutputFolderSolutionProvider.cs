using System;

namespace FineCodeCoverage.Engine
{
    internal interface ICoverageToolOutputFolderSolutionProvider
    {
        string Provide(Func<string> solutionFolderProvider);
    }
}
