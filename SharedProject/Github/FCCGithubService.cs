using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Core.Utilities.FCCVersioning;
using FineCodeCoverage.Output.Pane;
using FineCodeCoverage.Readme;
using FineCodeCoverage.Wpf;
using WpfHelpers;

namespace FineCodeCoverage.Github
{
    [Export(typeof(IFCCGithubService))]
    internal class FCCGithubService : ObservableBase, IFCCGithubService, INewIssueViewModel
    {
        private readonly IFCCOutputWindowPaneCreator _paneCreator;
        private readonly IVsVersion _vsVersion;
        private readonly IFCCVersion _fccVersion;
        private readonly IProcess _process;
        private readonly RelayCommand _submitCommand;
        private readonly RelayCommand _mailToCommand;
        private readonly RelayCommand _searchIssuesCommand;
        private readonly RelayCommand _openReadMeCommand;
        private readonly RelayCommand _refreshFCCOutputCommand;
        private string _fccOutput;
        private string _title;
        private bool _haveReadReadme;
        private bool _haveCheckedFCCIssues;

        public string VsVersionString { get; private set; }
        public string FccVersionString { get; private set; }

        public string FccOutput
        {
            get => this._fccOutput;
            set
            {
                this.Set(ref this._fccOutput, value);
                this._mailToCommand.NotifyCanExecuteChanged();
            }
        }

        public string Title
        {
            get => this._title;
            set
            {
                this.Set(ref this._title, value);
                this._submitCommand.NotifyCanExecuteChanged();
            }
        }

        public bool HaveReadReadme
        {
            get => this._haveReadReadme;
            set
            {
                this.Set(ref this._haveReadReadme, value);
                this._submitCommand.NotifyCanExecuteChanged();
            }
        }

        public bool HaveCheckedFCCIssues
        {
            get => this._haveCheckedFCCIssues;
            set
            {
                this.Set(ref this._haveCheckedFCCIssues, value);
                this._submitCommand.NotifyCanExecuteChanged();
            }
        }

        public ICommand SubmitCommand => this._submitCommand;
        public ICommand MailToCommand => this._mailToCommand;
        public ICommand SearchIssuesCommand => this._searchIssuesCommand;
        public ICommand OpenReadMeCommand => this._openReadMeCommand;
        public ICommand RefreshFCCOutputCommand => this._refreshFCCOutputCommand;

        [ImportingConstructor]
        public FCCGithubService(
            IFCCOutputWindowPaneCreator paneCreator,
            IVsVersion vsVersion,
            IFCCVersion fccVersion,
            IProcess process,
            IUrlEncoder urlEncoder,
            IShowReadMeService readMeService
        )
        {
            this._paneCreator = paneCreator;
            this._vsVersion = vsVersion;
            this._fccVersion = fccVersion;
            this._process = process;
            this._submitCommand = new RelayCommand(() =>
            {
                var encodings = new Dictionary<string, string>
                {
                    { "vsversion", this.VsVersionString },
                    { "fccversion", this.FccVersionString },
                    { "title", this.Title }
                };

                var sb = new StringBuilder($"{FCCGithub.Repo}/issues/new?template=Issue-form.yaml");
                _ = encodings.Aggregate(sb, (acc, kv) => acc.Append($"&{kv.Key}={urlEncoder.Encode(kv.Value)}"));

                Clipboard.SetDataObject(this.FccOutput);
                string url = sb.ToString();
                process.Start(url);
            }, () => !string.IsNullOrWhiteSpace(this.Title) && this.HaveReadReadme && this.HaveCheckedFCCIssues);
            this._mailToCommand = new RelayCommand(() =>
            {
                string mailto = string.Format("mailto:{0}?Subject={1}&Body={2}", "fortunengwenya@gmail.com", this.Title, this.FccOutput);
                mailto = Uri.EscapeUriString(mailto);
                process.Start(mailto);
            }, () => !string.IsNullOrWhiteSpace(this.FccOutput));
            this._openReadMeCommand = new RelayCommand(() => readMeService.Show());
            this._searchIssuesCommand = new RelayCommand(() =>
                {
                    process.Start($"{FCCGithub.Repo}/issues?q=is%3Aissue+{urlEncoder.Encode(this.Title)}");
                    this.HaveCheckedFCCIssues = true;
                }
            );
            this._refreshFCCOutputCommand = new RelayCommand(() => _ = this.GetFCCOutputAsync());

            this.HaveReadReadme = readMeService.HasShown;
            readMeService.Shown += (s, e) => this.HaveReadReadme = true;
        }

        public void NewIssue() => _ = this.NewIssueAsync();

        private async Task NewIssueAsync()
        {
            if (this.VsVersionString == null)
            {
                this.VsVersionString = $"{this._vsVersion.GetEditionName()} {this._vsVersion.GetDisplayVersion()}";
                this.FccVersionString = this._fccVersion.GetVersion();
            }

            await this.GetFCCOutputAsync();
            new NewIssueDialogWindow(this).Show();
        }

        private async Task GetFCCOutputAsync()
        {
            IFCCOutputWindowPane pane = await this._paneCreator.GetOrCreateAsync();
            this.FccOutput = await pane.GetTextAsync();
        }

        public void Navigate() => this._process.Start(FCCGithub.Repo);
    }
}