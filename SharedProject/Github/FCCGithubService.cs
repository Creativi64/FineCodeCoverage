using FineCodeCoverage.Core.Utilities.FCCVersioning;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output.Pane;
using System.ComponentModel.Composition;
using TreeGrid;
using FineCodeCoverage.Wpf;
using System.Windows.Input;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using FineCodeCoverage.Readme;
using System.Threading.Tasks;

namespace FineCodeCoverage.Github
{
    [Export(typeof(IFCCGithubService))]
    internal class FCCGithubService : ObservableBase, IFCCGithubService, INewIssueViewModel
    {
        private readonly IFCCOutputWindowPaneCreator paneCreator;
        private readonly IVsVersion vsVersion;
        private readonly IFCCVersion fccVersion;
        private readonly IProcess process;
        private const string fccRepo = "https://github.com/FortuneN/FineCodeCoverage";
        private readonly RelayCommand submitCommand;
        private readonly RelayCommand mailToCommand;
        private readonly RelayCommand searchIssuesCommand;
        private readonly RelayCommand openReadMeCommand;
        private readonly RelayCommand refreshFCCOutputCommand;

        public string VsVersionString { get; private set; }
        public string FccVersionString { get; private set; }
        private string fccOutput;
        public string FccOutput {
            get => this.fccOutput;
            set
            {
                this.Set(ref this.fccOutput, value);
                this.mailToCommand.NotifyCanExecuteChanged();
            }
        }

        private string title;
        public string Title
        {
            get => this.title;
            set
            {
                this.Set(ref this.title, value);
                this.submitCommand.NotifyCanExecuteChanged();
            }
        }

        private bool haveReadReadme;
        public bool HaveReadReadme
        {
            get => this.haveReadReadme;
            set
            {
                this.Set(ref this.haveReadReadme, value);
                this.submitCommand.NotifyCanExecuteChanged();
            }
        }

        private bool haveCheckedFCCIssues;
        

        public bool HaveCheckedFCCIssues
        {
            get => this.haveCheckedFCCIssues;
            set
            {
                this.Set(ref this.haveCheckedFCCIssues, value);
                this.submitCommand.NotifyCanExecuteChanged();
            }
        }

        public ICommand SubmitCommand => this.submitCommand;
        public ICommand MailToCommand => this.mailToCommand;
        public ICommand SearchIssuesCommand => this.searchIssuesCommand;
        public ICommand OpenReadMeCommand => this.openReadMeCommand;
        public ICommand RefreshFCCOutputCommand => this.refreshFCCOutputCommand;

        [ImportingConstructor]
        public FCCGithubService(
            IFCCOutputWindowPaneCreator paneCreator,
            IVsVersion vsVersion,
            IFCCVersion fccVersion,
            IProcess process,
            IUrlEncoder urlEncoder,
            IReadMeService readMeService
        )
        {
            this.paneCreator = paneCreator;
            this.vsVersion = vsVersion;
            this.fccVersion = fccVersion;
            this.process = process;
            this.submitCommand = new RelayCommand(() =>
            {
                var encodings = new Dictionary<string, string>
                {
                    { "vsversion", this.VsVersionString },
                    { "fccversion", this.FccVersionString },
                    { "title", this.Title }
                };

                var sb = new StringBuilder($"{fccRepo}/issues/new?template=Issue-form.yaml");
                _ = encodings.Aggregate(sb, (acc, kv) => acc.Append($"&{kv.Key}={urlEncoder.Encode(kv.Value)}"));

                Clipboard.SetDataObject(this.FccOutput);
                string url = sb.ToString();
                process.Start(url);
            }, () => !string.IsNullOrWhiteSpace(this.Title) && this.HaveReadReadme && this.HaveCheckedFCCIssues);
            this.mailToCommand = new RelayCommand(() =>
            {
                string mailto = string.Format("mailto:{0}?Subject={1}&Body={2}", "fortunengwenya@gmail.com", this.Title, this.FccOutput);
                mailto = Uri.EscapeUriString(mailto);
                process.Start(mailto);
            }, () => !string.IsNullOrWhiteSpace(this.FccOutput));
            this.openReadMeCommand = new RelayCommand(() => readMeService.ShowReadMe());
            this.searchIssuesCommand = new RelayCommand(() =>
                {
                    process.Start($"{fccRepo}/issues?q=is%3Aissue+{urlEncoder.Encode(this.Title)}");
                    this.HaveCheckedFCCIssues = true;
                }
            );
            this.refreshFCCOutputCommand = new RelayCommand(() => _ = this.GetFCCOutputAsync());

            this.HaveReadReadme = readMeService.HasShownReadMe;
            readMeService.ReadMeShown += (s, e) => this.HaveReadReadme = true;
        }

        public void NewIssue() => _ = this.NewIssueAsync();

        private async Task NewIssueAsync()
        {
            if (this.VsVersionString == null)
            {
                this.VsVersionString = $"{this.vsVersion.GetEditionName()} {this.vsVersion.GetDisplayVersion()}";
                this.FccVersionString = this.fccVersion.GetVersion();
            }

            await this.GetFCCOutputAsync();
            new NewIssueDialogWindow(this).Show();
        }

        private async Task GetFCCOutputAsync()
        {
            IFCCOutputWindowPane pane = await this.paneCreator.GetOrCreateAsync();
            this.FccOutput = await pane.GetTextAsync();
        }

        public void Navigate() => this.process.Start(fccRepo);
    }
}
