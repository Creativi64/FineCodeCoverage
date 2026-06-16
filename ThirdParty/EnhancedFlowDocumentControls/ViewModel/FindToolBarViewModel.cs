using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using EnhancedFlowDocumentControls.Commands;

namespace EnhancedFlowDocumentControls.ViewModel
{
    internal sealed class FindToolBarViewModel : INotifyPropertyChanged, IFindableToolBarViewModel
    {
        private readonly IFinder _finder;
        private string _findText;
        private bool _allowSearchingWhenEmptyText;
        private bool _isSearchUp;
        private bool _matchWholeWord;
        private bool _matchCase;
        private bool _matchDiacritic;
        private bool _matchKashida;
        private bool _matchAlefHamza;
        private object _originalDataContext;

        public event PropertyChangedEventHandler PropertyChanged;

        private sealed class FindParameterImpl<T> : IFindParameter<T>
        {
            private T _originalValue = default;

            public bool Changed { get; private set; }

            public T Value { get; private set; }

            public void Reset()
            {
                _originalValue = Value;
                Changed = false;
            }

            public void SetValue(T value)
            {
                if (EqualityComparer<T>.Default.Equals(_originalValue, value))
                {
                    Changed = false;
                    return;
                }

                Value = value;
                Changed = true;
            }
        }

        private sealed class FindParameters : IFindParameters
        {
            public FindParameters(FindToolBarViewModel findToolBarViewModel)
                => findToolBarViewModel.PropertyChanged += FindToolBarViewModel_PropertyChanged;

            private void FindToolBarViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (!(sender is FindToolBarViewModel findToolBarViewModel))
                {
                    return;
                }

                switch (e.PropertyName)
                {
                    case nameof(FindToolBarViewModel.FindText):
                        FindTextImpl.SetValue(findToolBarViewModel.FindText);
                        break;
                    case nameof(FindToolBarViewModel.IsSearchUp):
                        IsSearchUpImpl.SetValue(findToolBarViewModel.IsSearchUp);
                        break;
                    case nameof(FindToolBarViewModel.MatchAlefHamza):
                        MatchAlefHamzaImpl.SetValue(findToolBarViewModel.MatchAlefHamza);
                        break;
                    case nameof(FindToolBarViewModel.MatchCase):
                        MatchCaseImpl.SetValue(findToolBarViewModel.MatchCase);
                        break;
                    case nameof(FindToolBarViewModel.MatchDiacritic):
                        MatchDiacriticImpl.SetValue(findToolBarViewModel.MatchDiacritic);
                        break;
                    case nameof(FindToolBarViewModel.MatchKashida):
                        MatchKashidaImpl.SetValue(findToolBarViewModel.MatchKashida);
                        break;
                    case nameof(FindToolBarViewModel.MatchWholeWord):
                        MatchWholeWordImpl.SetValue(findToolBarViewModel.MatchWholeWord);
                        break;
                }
            }

            public FindParameterImpl<string> FindTextImpl { get; } = new FindParameterImpl<string>();

            public FindParameterImpl<bool> IsSearchUpImpl { get; } = new FindParameterImpl<bool>();

            public FindParameterImpl<bool> MatchAlefHamzaImpl { get; set; } = new FindParameterImpl<bool>();

            public FindParameterImpl<bool> MatchCaseImpl { get; set; } = new FindParameterImpl<bool>();

            public FindParameterImpl<bool> MatchDiacriticImpl { get; set; } = new FindParameterImpl<bool>();

            public FindParameterImpl<bool> MatchKashidaImpl { get; set; } = new FindParameterImpl<bool>();

            public FindParameterImpl<bool> MatchWholeWordImpl { get; set; } = new FindParameterImpl<bool>();

            public IFindParameter<string> FindText => FindTextImpl;

            public IFindParameter<bool> IsSearchUp => IsSearchUpImpl;

            public IFindParameter<bool> MatchAlefHamza => MatchAlefHamzaImpl;

            public IFindParameter<bool> MatchCase => MatchCaseImpl;

            public IFindParameter<bool> MatchDiacritic => MatchDiacriticImpl;

            public IFindParameter<bool> MatchKashida => MatchKashidaImpl;

            public IFindParameter<bool> MatchWholeWord => MatchWholeWordImpl;
        }

        private readonly FindParameters _findParameters;

        public object OriginalDataContext
        {
            get => _originalDataContext;
            private set
            {
                if (_originalDataContext == value)
                {
                    return;
                }

                _originalDataContext = value;
                OnPropertyChanged(nameof(OriginalDataContext));
            }
        }

        internal FindToolBarViewModel(IFinder finder, FrameworkElement originalDataContextElement)
        {
            _findParameters = new FindParameters(this);
            _finder = finder;
            NextCommand = new RelayCommand(FindNext, CanFind);
            PreviousCommand = new RelayCommand(FindPrevious, CanFind);
            SetUpOriginalDataContextIfRequired(originalDataContextElement);
        }

        private void SetUpOriginalDataContextIfRequired(FrameworkElement originalDataContextElement)
        {
            if (originalDataContextElement == null)
            {
                return;
            }

            OriginalDataContext = originalDataContextElement.DataContext;
            originalDataContextElement.DataContextChanged += (s, e) => OriginalDataContext = originalDataContextElement.DataContext;
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
                OnPropertyChanged(nameof(FindText));
                RaisesCommandCanExecuteChanged();
            }
        }

        #region menu items
        public bool MatchWholeWord
        {
            get => _matchWholeWord;
            set => SetProperty(ref _matchWholeWord, value);
        }

        public bool MatchCase
        {
            get => _matchCase;
            set => SetProperty(ref _matchCase, value);
        }

        public bool MatchDiacritic
        {
            get => _matchDiacritic;
            set => SetProperty(ref _matchDiacritic, value);
        }

        public bool MatchKashida
        {
            get => _matchKashida;
            set => SetProperty(ref _matchKashida, value);
        }

        public bool MatchAlefHamza
        {
            get => _matchAlefHamza;
            set => SetProperty(ref _matchAlefHamza, value);
        }
        #endregion

        #region IsSearchUp / IsSearchDown
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
                OnPropertyChanged(nameof(IsSearchDown));
            }
        }

        public bool IsSearchDown => !IsSearchUp;

        #endregion

        #region find commands
        public ICommand NextCommand { get; }

        public ICommand PreviousCommand { get; }

        public bool AllowSearchingWhenEmptyText
        {
            get => _allowSearchingWhenEmptyText;
            set
            {
                _allowSearchingWhenEmptyText = value;
                RaisesCommandCanExecuteChanged();
            }
        }

        private void FindNext() => Find(false);

        private void FindPrevious() => Find(true);

        private bool CanFind() => AllowSearchingWhenEmptyText || !string.IsNullOrWhiteSpace(FindText);

        private void RaisesCommandCanExecuteChanged()
        {
            (NextCommand as RelayCommand).NotifyCanExecuteChanged();
            (PreviousCommand as RelayCommand).NotifyCanExecuteChanged();
        }
        #endregion

        public void Find(bool searchUp)
        {
            IsSearchUp = searchUp;
            Find();
        }

        public void Find() => _finder.Find(_findParameters);

        #region INotifyPropertyChanged
        private void SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion
        public void ApplySettings(IFindToolBarSettings retainedSettings)
        {
            FindText = retainedSettings.FindText;
            IsSearchUp = retainedSettings.IsSearchUp;
            MatchWholeWord = retainedSettings.MatchWholeWord;
            MatchCase = retainedSettings.MatchCase;
            MatchDiacritic = retainedSettings.MatchDiacritic;
            MatchKashida = retainedSettings.MatchKashida;
            MatchAlefHamza = retainedSettings.MatchAlefHamza;
        }
    }
}
