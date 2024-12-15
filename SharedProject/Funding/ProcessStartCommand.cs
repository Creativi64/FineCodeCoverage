using FineCodeCoverage.Output;
using System;
using System.Windows.Input;

namespace FineCodeCoverage.Funding
{
    internal class ProcessStartCommand : ICommand
    {
        private readonly IProcess process;

        public ProcessStartCommand(IProcess process) => this.process = process;
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => this.process.Start(parameter.ToString());
    }
}
