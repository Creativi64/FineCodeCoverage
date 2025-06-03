using System.Collections.Generic;

namespace FineCodeCoverage.Output
{
    internal interface IGitService
    {
        IReadOnlyList<string> GetRepositoryPaths();
        IGitRepo GetRepository(string selectedRepository);
        IChangeset GetChangeset(IDictionary<string, HashSet<int>> changeLookup);
        bool CanUseChangeset { get; }
    }
}