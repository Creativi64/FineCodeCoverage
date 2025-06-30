using FineCodeCoverage.Funding;
using FineCodeCoverage.Github;

namespace FineCodeCoverage.Utilities.Vs
{
    public interface IDialogWindowService
    {
        void ShowNewIssueDialogWindow(INewIssueViewModel viewModel);
        void ShowFundingDialogWindow(IFundingViewModel viewModel);
    }
}
