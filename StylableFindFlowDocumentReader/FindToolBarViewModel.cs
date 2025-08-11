using System.ComponentModel;
using System.Windows.Input;

namespace StylableFindFlowDocumentReader
{
    public class FindToolBarViewModel : INotifyPropertyChanged
    {
        private readonly FindToolbarWrapper _wrapper;
        private string _findText;
        private bool _isSearchUp;
        private bool _matchWholeWord;
        private bool _matchCase;
        private bool _matchDiacritic;
        private bool _matchKashida;
        private bool _matchAlefHamza;

        public FindToolBarViewModel(FindToolbarWrapper wrapper)
        {
            _wrapper = wrapper;
            NextCommand = new RelayCommand(ExecuteNext, CanExecuteFind);
            PreviousCommand = new RelayCommand(ExecutePrevious, CanExecuteFind);
        }

        public string FindText
        {
            get => _findText;
            set
            {
                if (_findText == value)
                {
                    return;
                }

                _findText = value;
                _wrapper.SetFindText(value);
                OnPropertyChanged(nameof(FindText));
                RaiseCommandCanExecuteChanged();
            }
        }

        public bool MatchWholeWord
        {
            get => _matchWholeWord;
            set
            {
                if (_matchWholeWord == value)
                {
                    return;
                }

                _matchWholeWord = value;
                _wrapper.SelectMatchWholeWord(value);
            }
        }

        public bool MatchCase
        {
            get => _matchCase;
            set
            {
                if (_matchCase == value)
                {
                    return;
                }

                _matchCase = value;
                _wrapper.SelectMatchCase(value);
            }
        }

        public bool MatchDiacritic
        {
            get => _matchDiacritic;
            set
            {
                if (_matchDiacritic == value)
                {
                    return;
                }

                _matchDiacritic = value;
                _wrapper.SelectMatchDiacritic(value);
            }
        }

        public bool MatchKashida
        {
            get => _matchKashida;
            set
            {
                if (_matchKashida == value)
                {
                    return;
                }

                _matchKashida = value;
                _wrapper.SelectMatchKashida(value);
            }
        }

        public bool MatchAlefHamza
        {
            get => _matchAlefHamza;
            set
            {
                if (_matchAlefHamza == value)
                {
                    return;
                }

                _matchAlefHamza = value;
                _wrapper.SelectMatchAlefHamza(value);
            }
        }

        public bool IsSearchUp
        {
            get => _isSearchUp;
            private set
            {
                if (_isSearchUp == value)
                {
                    return;
                }

                _isSearchUp = value;
                OnPropertyChanged(nameof(IsSearchUp));
            }
        }

        public ICommand NextCommand { get; }

        public ICommand PreviousCommand { get; }

        private void ExecuteNext() => Find(false);

        private void ExecutePrevious() => Find(true);

        internal void Find(bool searchUp)
        {
            _wrapper.SetSearchUp(searchUp);
            IsSearchUp = searchUp;
            _wrapper.Find();
        }

        internal void Find() => _wrapper.Find();

        private bool CanExecuteFind() => !string.IsNullOrWhiteSpace(FindText);

        private void RaiseCommandCanExecuteChanged()
        {
            (NextCommand as RelayCommand).NotifyCanExecuteChanged();
            (PreviousCommand as RelayCommand).NotifyCanExecuteChanged();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
