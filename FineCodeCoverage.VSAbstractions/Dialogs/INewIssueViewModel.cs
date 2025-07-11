using System.ComponentModel;
using System.Windows.Input;
using VsThemedDialogs;

namespace FineCodeCoverage.Github
{
    public interface INewIssueViewModel : INotifyPropertyChanged, IDialogViewModel
    {
        string VsVersionString { get; }

        string FccVersionString { get; }

        string Title { get; set; }

        string FccOutput { get; set; }

        bool HaveCheckedFCCIssues { get; set; }

        bool HaveReadReadme { get; set; }

        ICommand OkCommand { get; }

        ICommand MailToCommand { get; }

        ICommand OpenReadMeCommand { get; }

        ICommand SearchIssuesCommand { get; }

        ICommand RefreshFCCOutputCommand { get; }
    }
}
