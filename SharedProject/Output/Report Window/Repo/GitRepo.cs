#if VS2022
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
#endif

namespace FineCodeCoverage.Output
{
    #if VS2022
    internal class GitRepo : IGitRepo
    {
        private readonly Repository _repository;
        private bool disposedValue;

        public GitRepo(string repository)
        {
            this._repository = new Repository(repository);
        }

        // might want a wrapper if FriendlyName is not distinct or expensive to get the branch again
        public IEnumerable<string> GetBranches()
        {
            List<string> branches = new List<string>();
            foreach (Branch branch in this._repository.Branches)
            {
                if (branch.Tip != null)
                {
                    if (branch.FriendlyName.StartsWith("main") || branch.FriendlyName.StartsWith("master") || branch.FriendlyName.StartsWith("origin/main") || branch.FriendlyName.StartsWith("origin/master"))
                        branches.Insert(0, branch.FriendlyName);
                    else
                        branches.Add(branch.FriendlyName);
                }
            }
            return branches;
        }

        public bool HasBranch(string selectedBranchName)
        {
            return GetBranches().Any(b => b == selectedBranchName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _repository.Dispose();
                }
                disposedValue = true;
            }
        }


        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
#endif
}
