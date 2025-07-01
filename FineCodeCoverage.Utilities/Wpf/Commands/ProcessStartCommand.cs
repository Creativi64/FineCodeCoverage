using System;
using System.Windows.Input;
using FineCodeCoverage.Utilities.Wrappers;

namespace FineCodeCoverage.Utilities.Wpf.Commands
{
    public sealed class ProcessStartCommand : ICommand
    {
        private readonly IProcess _process;

        public ProcessStartCommand(IProcess process) => _process = process;

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => _process.Start(parameter as string);
    }
}
