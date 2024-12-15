using System;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace FineCodeCoverage.Wpf
{

    public sealed class RelayCommand : ICommand
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action execute) => this.execute = execute;

        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanExecute(object _) => this.canExecute?.Invoke() != false;

        public void Execute(object _) => this.execute();
    }
}