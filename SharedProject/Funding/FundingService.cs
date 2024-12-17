using System.ComponentModel.Composition;
using System.Windows.Input;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Funding
{
    [Export(typeof(IFundingService))]
    internal class FundingService : IFundingService, IFundingViewModel
    {
        [ImportingConstructor]
        public FundingService(IProcess process)
        {
            this.KofiClickedCommand = new ProcessStartCommand(process);
            this.BuyMeACoffeeClickedCommand = new ProcessStartCommand(process);
            this.LiberapayClickedCommand = new ProcessStartCommand(process);
            this.PayPalClickedCommand = new ProcessStartCommand(process);
            this.GithubClickedCommand = new ProcessStartCommand(process);
        }
        public ICommand KofiClickedCommand { get; }
        public ICommand BuyMeACoffeeClickedCommand { get; }
        public ICommand LiberapayClickedCommand { get; }
        public ICommand PayPalClickedCommand { get; }
        public ICommand GithubClickedCommand { get; }

        public void Execute() => _ = new FundingDialogWindow(this).ShowModal();
    }
}
