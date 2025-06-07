using System.ComponentModel.Composition;
using System.Windows.Input;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Wpf;

namespace FineCodeCoverage.Funding
{
    [Export(typeof(IFundingService))]
    internal sealed class FundingService : IFundingService, IFundingViewModel
    {
        [ImportingConstructor]
        public FundingService(IProcess process)
        {
            KofiClickedCommand = new ProcessStartCommand(process);
            BuyMeACoffeeClickedCommand = new ProcessStartCommand(process);
            LiberapayClickedCommand = new ProcessStartCommand(process);
            PayPalClickedCommand = new ProcessStartCommand(process);
            GithubClickedCommand = new ProcessStartCommand(process);
        }

        public ICommand KofiClickedCommand { get; }

        public ICommand BuyMeACoffeeClickedCommand { get; }

        public ICommand LiberapayClickedCommand { get; }

        public ICommand PayPalClickedCommand { get; }

        public ICommand GithubClickedCommand { get; }

        public void Execute() => _ = new FundingDialogWindow(this).ShowModal();
    }
}
