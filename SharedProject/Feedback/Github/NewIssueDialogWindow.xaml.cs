using FineCodeCoverage.VSAbstractions.Dialogs;
using VsThemedDialogs;

namespace FineCodeCoverage.Feedback.Github
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
