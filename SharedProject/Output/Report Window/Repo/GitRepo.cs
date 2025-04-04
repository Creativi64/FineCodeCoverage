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
                    if (IsMain(branch))
                        branches.Insert(0, branch.FriendlyName);
                    else
                        branches.Add(branch.FriendlyName);
                }
            }
            return branches;
        }

        private static bool IsMain(Branch branch)
        {
            return branch.FriendlyName.StartsWith("main") || branch.FriendlyName.StartsWith("master") || branch.FriendlyName.StartsWith("origin/main") || branch.FriendlyName.StartsWith("origin/master");
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

        public IDictionary<string, HashSet<int>> GetChangeset(string selectedBranchName)
        {
            var selectedBranch = this._repository.Branches.FirstOrDefault(branch => branch.FriendlyName == selectedBranchName);
            if (selectedBranch == null) return null;

            Dictionary<string, HashSet<int>> changeset = new Dictionary<string, HashSet<int>>();
            this.AddChanges(this._repository.Diff.Compare<Patch>(), changeset);
            this.AddChanges(this._repository.Diff.Compare<Patch>(selectedBranch.Tip.Tree, DiffTargets.Index), changeset);
            return changeset;
        }

        private void AddChanges(Patch patch, Dictionary<string, HashSet<int>> changeset)
        {
            foreach (PatchEntryChanges patchEntryChanges in patch)
            {
                switch (patchEntryChanges.Status)
                {
                    case ChangeKind.Added:
                    case ChangeKind.Modified:
                    case ChangeKind.Renamed:
                        string key = patchEntryChanges.Path.Replace("/", "\\");
                        HashSet<int> intSet;
                        if (!changeset.TryGetValue(key, out intSet))
                        {
                            intSet = new HashSet<int>();
                            changeset[key] = intSet;
                        }
                        using (List<Line>.Enumerator enumerator = patchEntryChanges.AddedLines.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                Line current = enumerator.Current;
                                intSet.Add(current.LineNumber);
                            }
                            continue;
                        }
                    default:
                        continue;
                }
            }
        }
    }
#endif
}
