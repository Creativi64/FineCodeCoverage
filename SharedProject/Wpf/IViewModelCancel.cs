using System.Windows.Input;

namespace FineCodeCoverage.Wpf
{
    public interface IViewModelCancel
    {
        ICommand CancelCommand { get; }
    }
}
