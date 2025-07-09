using System.Windows.Input;

namespace FineCodeCoverage.Wpf
{
    public interface IDialogViewModel : IViewModelCancel, IViewModelDone
    {
        ICommand OkCommand { get; }
    }
}
