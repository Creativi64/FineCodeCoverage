using System.ComponentModel;
using System.Windows.Input;

namespace FineCodeCoverage.Github
{
    public interface INewIssueViewModel : INotifyPropertyChanged
    {
        string VsVersionString { get; }
        string FccVersionString { get; }
        string Title { get; set; }
        string FccOutput { get; set; }
        bool HaveCheckedFCCIssues { get; set; }
        bool HaveReadReadme { get; set; }
        ICommand SubmitCommand { get; }
        ICommand MailToCommand { get; }
        ICommand OpenReadMeCommand { get; }
        ICommand SearchIssuesCommand { get; }
        ICommand RefreshFCCOutputCommand { get; }
    }
}
