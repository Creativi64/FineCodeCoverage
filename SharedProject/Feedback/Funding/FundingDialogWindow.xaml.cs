using FineCodeCoverage.VSAbstractions.Dialogs;
using VsThemedDialogs;

namespace FineCodeCoverage.Funding
{
    /// <summary>
    /// FundingDialogWindow.xaml.cs.
    /// </summary>
    public partial class FundingDialogWindow : ThemedDialogWindow
    {
        public FundingDialogWindow(IFundingViewModel fundingViewModel)
        {
            DataContext = fundingViewModel;
            InitializeComponent();
        }
    }
}
