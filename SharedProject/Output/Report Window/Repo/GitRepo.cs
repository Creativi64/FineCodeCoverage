#if VS2022
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
#endif

namespace FineCodeCoverage.Output
{
#if VS2022
    internal class GitRepo : IGitRepo
    {
        private readonly Repository _repository;
        private readonly string _workingDirectory;
        private bool _disposedValue;

        public GitRepo(string repository)
        {
            this._repository = new Repository(repository);
            this._workingDirectory = this._repository.Info.WorkingDirectory;
        }

        // might want a wrapper if FriendlyName is not distinct or expensive to get the branch again
        public IEnumerable<string> GetBranches()
        {
            var branches = new List<string>();
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
            => branch.FriendlyName.StartsWith("main") ||
            branch.FriendlyName.StartsWith("master") ||
            branch.FriendlyName.StartsWith("origin/main")
            || branch.FriendlyName.StartsWith("origin/master");

        public bool HasBranch(string selectedBranchName) => this.GetBranches().Any(b => b == selectedBranchName);

        protected virtual void Dispose(bool disposing)
        {
            if (this._disposedValue)
            {
                return;
            }

            if (disposing)
            {
                this._repository.Dispose();
            }

            this._disposedValue = true;
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public IDictionary<string, HashSet<int>> GetChangeset(string selectedBranchName)
        {
            Branch selectedBranch = this._repository.Branches.FirstOrDefault(branch => branch.FriendlyName == selectedBranchName);
            if (selectedBranch == null) return null;

            var changeset = new Dictionary<string, HashSet<int>>();
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
                        if (this._workingDirectory != null)
                        {
                            key = Path.Combine(this._workingDirectory, key);
                        }

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
                                _ = intSet.Add(current.LineNumber);
                            }

                            continue;
                        }
                    default:
                        continue;
                }
            }
        }

        public bool Deleted()
        {
            try
            {
                bool _ = this._repository.Info.IsHeadDetached;
            }
            catch
            {
                return true;
            }

            return false;
        }
    }
#endif
}
