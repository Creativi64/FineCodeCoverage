using System.Collections.Generic;

namespace FineCodeCoverage.Output
{
    internal class ReportViewState
    {
        public ReportViewState(
            ReportViewSolutionOptionValue optionValue,
            IReadOnlyList<string> repositories,
            bool canUseRepositories
        )
        {
            this.ReportStyle = optionValue.ReportStyle;
            this.ReportContentType = optionValue.ReportContent;
            this.SelectedRepositoryPath = optionValue.SelectedRepository;
            this.SelectedBranchName = optionValue.SelectedBranchName;
            this.RepositoryPaths = repositories;
            this.CanUseRepositories = canUseRepositories;
        }

        public ReportStyle ReportStyle { get; }
        public ReportContentType ReportContentType { get; }
        public string SelectedRepositoryPath { get; }
        public string SelectedBranchName { get; }
        public IReadOnlyList<string> RepositoryPaths { get; }
        public bool CanUseRepositories { get; }
    }
}