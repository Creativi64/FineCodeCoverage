#if VS2022
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using Microsoft;
using System.Linq;
#endif
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output
{
#if VS2022
    [Export(typeof(IGitService))]
    internal class GitService : IGitService
    {
        private readonly IGitExt _gitExt;

        [ImportingConstructor]
        public GitService(
             [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider
        )
        {
            _gitExt = serviceProvider.GetService(typeof(IGitExt)) as IGitExt;
            Assumes.Present(_gitExt);
        }

        public IReadOnlyList<string> GetRepositoryPaths()
            => _gitExt.ActiveRepositories.Select(r => r.RepositoryPath).ToList();

        public IGitRepo GetRepository(string selectedRepositoryPath)
        {
            try
            {
                return new GitRepo(selectedRepositoryPath);
            }
            catch (LibGit2Sharp.RepositoryNotFoundException)
            {
                return null;
            }
        }

        public IChangeset GetChangeset(IDictionary<string, HashSet<int>> changeLookup) => new Changeset(changeLookup);

        public bool CanUseChangeset => true;
    }
#else
    [Export(typeof(IGitService))]
    internal class GitService2019 : IGitService
    {
        public IGitRepo GetRepository(string selectedRepository) => throw new NotImplementedException();

        public IChangeset GetChangeset(IDictionary<string, HashSet<int>> changeLookup) => throw new NotImplementedException();

        public IReadOnlyList<string> GetRepositoryPaths() => throw new NotImplementedException();

        public bool CanUseChangeset => false;
    }
#endif
}
