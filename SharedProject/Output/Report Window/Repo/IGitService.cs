using System.Collections.Generic;

namespace FineCodeCoverage.Output
{
    internal interface IGitService
    {
        List<string> GetRepositoryPaths();
        IGitRepo GetRepository(string selectedRepository);
    }
}
