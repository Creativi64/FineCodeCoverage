using System.Windows.Input;

namespace FineCodeCoverage.Funding
{
    public interface IFundingViewModel
    {
        ICommand KofiClickedCommand { get; }
        ICommand BuyMeACoffeeClickedCommand { get; }
        ICommand LiberapayClickedCommand { get; }
        ICommand PayPalClickedCommand { get; }
    }
}
