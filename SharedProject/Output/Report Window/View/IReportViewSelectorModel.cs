using System.Collections.Generic;

namespace FineCodeCoverage.Output
{
    internal interface IReportViewSelectorModel
    {
        ReportViewState GetState();
        void Update(ReportStyle reportStyle,
            ReportContentType reportContentType,
            string selectedBranchName,
            string selectedRepositoryPath);
        IEnumerable<string> GetBranches(string selectedRepositoryPath);
    }
}