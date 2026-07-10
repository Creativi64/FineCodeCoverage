using System.Windows.Input;

namespace VsThemedDialogs
{
    public interface IDialogViewModel : IViewModelCancel, IViewModelDone
    {
        ICommand OkCommand { get; }
    }
}