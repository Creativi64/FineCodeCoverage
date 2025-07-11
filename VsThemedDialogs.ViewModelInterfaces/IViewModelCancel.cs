using System.Windows.Input;

namespace VsThemedDialogs
{
    public interface IViewModelCancel
    {
        ICommand CancelCommand { get; }
    }
}
