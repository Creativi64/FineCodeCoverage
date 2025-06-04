using System;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace FineCodeCoverage.Wpf
{
    public sealed class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action execute) => this._execute = execute;

        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            this._execute = execute;
            this._canExecute = canExecute;
        }

        public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanExecute(object parameter) => this._canExecute?.Invoke() != false;

        public void Execute(object parameter) => this._execute();
    }
}
