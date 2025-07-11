using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FineCodeCoverage.Readme;
using FineCodeCoverage.Utilities.Wrappers;
using FineCodeCoverage.VSAbstractions.Dialogs;
using FineCodeCoverage.VSAbstractions.OutputWindow;
using FineCodeCoverage.VSAbstractions.Versioning;
using WpfHelpers;

namespace FineCodeCoverage.Feedback.Github
{
    [Export(typeof(IFCCGithubService))]
    internal sealed class FCCGithubService : ObservableBase, IFCCGithubService, INewIssueViewModel
    {
        private readonly IFCCOutputWindowPaneCreator _paneCreator;
        private readonly IVsVersion _vsVersion;
        private readonly IFCCVersion _fccVersion;
        private readonly IProcess _process;
        private readonly IDialogWindowService _dialogWindowService;
        private readonly RelayCommand _submitCommand;
        private readonly RelayCommand _mailToCommand;
        private readonly RelayCommand _searchIssuesCommand;
        private readonly RelayCommand _openReadMeCommand;
        private readonly RelayCommand _refreshFCCOutputCommand;
        private string _fccOutput;
        private string _title;
        private bool _haveReadReadme;
        private bool _haveCheckedFCCIssues;

        public event EventHandler<bool> Done;

        public string VsVersionString { get; private set; }

        public string FccVersionString { get; private set; }

        public string FccOutput
        {
            get => _fccOutput;
            set
            {
                Set(ref _fccOutput, value);
                _mailToCommand.NotifyCanExecuteChanged();
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                Set(ref _title, value);
                _submitCommand.NotifyCanExecuteChanged();
            }
        }

        public bool HaveReadReadme
        {
            get => _haveReadReadme;
            set
            {
                Set(ref _haveReadReadme, value);
                _submitCommand.NotifyCanExecuteChanged();
            }
        }

        public bool HaveCheckedFCCIssues
        {
            get => _haveCheckedFCCIssues;
            set
            {
                Set(ref _haveCheckedFCCIssues, value);
                _submitCommand.NotifyCanExecuteChanged();
            }
        }

        public ICommand OkCommand => _submitCommand;

        public ICommand CancelCommand { get; }

        public ICommand MailToCommand => _mailToCommand;

        public ICommand SearchIssuesCommand => _searchIssuesCommand;

        public ICommand OpenReadMeCommand => _openReadMeCommand;

        public ICommand RefreshFCCOutputCommand => _refreshFCCOutputCommand;

        [ImportingConstructor]
        public FCCGithubService(
            IFCCOutputWindowPaneCreator paneCreator,
            IVsVersion vsVersion,
            IFCCVersion fccVersion,
            IProcess process,
            IUrlEncoder urlEncoder,
            IShowReadMeService readMeService,
            IDialogWindowService dialogWindowService)
        {
            _paneCreator = paneCreator;
            _vsVersion = vsVersion;
            _fccVersion = fccVersion;
            _process = process;
            _dialogWindowService = dialogWindowService;
            CancelCommand = new RelayCommand(() => Done?.Invoke(this, false), () => true);
            _submitCommand = new RelayCommand(
                () =>
                {
                    var encodings = new Dictionary<string, string>
                    {
                        { "vsversion", VsVersionString },
                        { "fccversion", FccVersionString },
                        { "title", Title },
                    };

                    var sb = new StringBuilder($"{FCCGithub.Repo}/issues/new?template=Issue-form.yaml");
                    _ = encodings.Aggregate(sb, (acc, kv) => acc.Append($"&{kv.Key}={urlEncoder.Encode(kv.Value)}"));

                    Clipboard.SetDataObject(FccOutput);
                    string url = sb.ToString();
                    process.Start(url);
                    Done?.Invoke(this, true);
                },
                () => !string.IsNullOrWhiteSpace(Title) && HaveReadReadme && HaveCheckedFCCIssues);
            _mailToCommand = new RelayCommand(
                () =>
                {
                    string mailto = string.Format("mailto:{0}?Subject={1}&Body={2}", "fortunengwenya@gmail.com", Title, FccOutput);
                    mailto = Uri.EscapeUriString(mailto);
                    process.Start(mailto);
                },
                () => !string.IsNullOrWhiteSpace(FccOutput));
            _openReadMeCommand = new RelayCommand(() => readMeService.Show());
            _searchIssuesCommand = new RelayCommand(() =>
                {
                    process.Start($"{FCCGithub.Repo}/issues?q=is%3Aissue+{urlEncoder.Encode(Title)}");
                    HaveCheckedFCCIssues = true;
                });
            _refreshFCCOutputCommand = new RelayCommand(() => _ = GetFCCOutputAsync());

            HaveReadReadme = readMeService.HasShown;
            readMeService.Shown += (s, e) => HaveReadReadme = true;
        }

        public void NewIssue() => _ = NewIssueAsync();

        private async Task NewIssueAsync()
        {
            if (VsVersionString == null)
            {
                VsVersionString = $"{_vsVersion.GetEditionName()} {_vsVersion.GetDisplayVersion()}";
                FccVersionString = _fccVersion.GetVersion();
            }

            await GetFCCOutputAsync();
            INewIssueViewModel viewModel = this;
            _dialogWindowService.ShowModal(viewModel);
        }

        private async Task GetFCCOutputAsync()
        {
            IFCCOutputWindowPane pane = await _paneCreator.GetOrCreateAsync();
            FccOutput = await pane.GetTextAsync();
        }

        public void Navigate() => _process.Start(FCCGithub.Repo);
    }
}
