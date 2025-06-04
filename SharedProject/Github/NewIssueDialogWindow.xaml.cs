using Microsoft.VisualStudio.PlatformUI;

namespace FineCodeCoverage.Github
{
    public partial class NewIssueDialogWindow : DialogWindow
    {
        public NewIssueDialogWindow(INewIssueViewModel newIssueViewModel)
        {
            HasMaximizeButton = true;
            HasMinimizeButton = true;
            DataContext = newIssueViewModel;
            InitializeComponent();
        }
    }
}
