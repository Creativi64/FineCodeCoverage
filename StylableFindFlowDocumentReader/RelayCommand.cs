using System;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace StylableFindFlowDocumentReader
{
    public sealed class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action execute) => _execute = execute;

        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanExecute(object parameter) => _canExecute?.Invoke() != false;

        public void Execute(object parameter) => _execute();
    }
}
