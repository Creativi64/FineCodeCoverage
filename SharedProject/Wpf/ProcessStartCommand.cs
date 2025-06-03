using System;
using System.Windows.Input;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Wpf
{
    internal class ProcessStartCommand : ICommand
    {
        private readonly IProcess process;

        public ProcessStartCommand(IProcess process) => this.process = process;

        public event EventHandler CanExecuteChanged { add { } remove { } }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => this.process.Start(parameter as string);
    }
}