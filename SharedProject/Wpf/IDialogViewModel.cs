using System;
using System.Windows.Input;

namespace FineCodeCoverage.Wpf
{
    public interface IDialogViewModel
    {
        event EventHandler Done;

        ICommand CancelCommand { get; }

        ICommand OkCommand { get; }
    }
}
