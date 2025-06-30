using System.ComponentModel.Composition;
using FineCodeCoverage.Funding;
using FineCodeCoverage.Github;
using FineCodeCoverage.Utilities.Vs;

namespace FineCodeCoverage.Vs
{
    [Export(typeof(IDialogWindowService))]
    internal sealed class DialogWindowService : IDialogWindowService
    {
        public void ShowFundingDialogWindow(IFundingViewModel viewModel) => new FundingDialogWindow(viewModel).ShowModal();

        public void ShowNewIssueDialogWindow(INewIssueViewModel viewModel) => new NewIssueDialogWindow(viewModel).Show();
    }
}
