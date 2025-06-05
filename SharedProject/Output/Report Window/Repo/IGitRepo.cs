using System;
using System.Collections.Generic;

namespace FineCodeCoverage.Output
{
    internal interface IGitRepo : IDisposable
    {
        bool HasBranch(string selectedBranchName);

        IEnumerable<string> GetBranches();

        IDictionary<string, HashSet<int>> GetChangeset(string selectedBranchName);

        bool Deleted();
    }
}
