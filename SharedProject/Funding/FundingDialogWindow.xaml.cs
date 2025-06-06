using Microsoft.VisualStudio.PlatformUI;

namespace FineCodeCoverage.Funding
{
    /// <summary>
    /// FundingDialogWindow.xaml.cs.
    /// </summary>
    public partial class FundingDialogWindow : DialogWindow
    {
        public FundingDialogWindow(IFundingViewModel fundingViewModel)
        {
            DataContext = fundingViewModel;
            InitializeComponent();
        }
    }
}
