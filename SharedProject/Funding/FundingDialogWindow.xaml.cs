using Microsoft.VisualStudio.PlatformUI;

namespace FineCodeCoverage.Funding
{
    public partial class FundingDialogWindow : DialogWindow
    {
        public FundingDialogWindow(IFundingViewModel fundingViewModel)
        {
            this.DataContext = fundingViewModel;
            this.InitializeComponent();
        }
    }
}
