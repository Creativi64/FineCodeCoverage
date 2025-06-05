using System.Collections.Generic;

namespace FineCodeCoverage.Output
{
    internal class ReportViewState
    {
        public ReportViewState(
            ReportViewSolutionOptionValue optionValue,
            IReadOnlyList<string> repositories,
            bool canUseRepositories)
        {
            ReportStyle = optionValue.ReportStyle;
            ReportContentType = optionValue.ReportContent;
            SelectedRepositoryPath = optionValue.SelectedRepository;
            SelectedBranchName = optionValue.SelectedBranchName;
            RepositoryPaths = repositories;
            CanUseRepositories = canUseRepositories;
        }

        public ReportStyle ReportStyle { get; }

        public ReportContentType ReportContentType { get; }

        public string SelectedRepositoryPath { get; }

        public string SelectedBranchName { get; }

        public IReadOnlyList<string> RepositoryPaths { get; }

        public bool CanUseRepositories { get; }
    }
}
