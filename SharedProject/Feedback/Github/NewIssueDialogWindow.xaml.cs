using FineCodeCoverage.Wpf;

namespace FineCodeCoverage.Github
{
    /// <summary>
    /// new issue dialog window.
    /// </summary>
    public partial class NewIssueDialogWindow : ThemedDialogWindow
    {
        public NewIssueDialogWindow(INewIssueViewModel newIssueViewModel)
        {
            DataContext = newIssueViewModel;
            InitializeComponent();
        }
    }
}
