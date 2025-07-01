using System.ComponentModel.Composition;
using System.Windows.Input;
using FineCodeCoverage.Funding;
using FineCodeCoverage.Utilities.Vs;
using FineCodeCoverage.Utilities.Wpf.Commands;
using FineCodeCoverage.Utilities.Wrappers;

namespace FineCodeCoverage.Feedback.Funding
{
    [Export(typeof(IFundingService))]
    internal sealed class FundingService : IFundingService, IFundingViewModel
    {
        private readonly IDialogWindowService dialogWindowService;

        [ImportingConstructor]
        public FundingService(IProcess process, IDialogWindowService dialogWindowService)
        {
            KofiClickedCommand = new ProcessStartCommand(process);
            BuyMeACoffeeClickedCommand = new ProcessStartCommand(process);
            LiberapayClickedCommand = new ProcessStartCommand(process);
            PayPalClickedCommand = new ProcessStartCommand(process);
            GithubClickedCommand = new ProcessStartCommand(process);
            this.dialogWindowService = dialogWindowService;
        }

        public ICommand KofiClickedCommand { get; }

        public ICommand BuyMeACoffeeClickedCommand { get; }

        public ICommand LiberapayClickedCommand { get; }

        public ICommand PayPalClickedCommand { get; }

        public ICommand GithubClickedCommand { get; }

        public void Execute() => dialogWindowService.ShowFundingDialogWindow(this);
    }
}
