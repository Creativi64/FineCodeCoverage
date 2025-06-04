using Microsoft.VisualStudio.PlatformUI;

namespace FineCodeCoverage.Github
{
    public partial class NewIssueDialogWindow : DialogWindow
    {
        public NewIssueDialogWindow(INewIssueViewModel newIssueViewModel)
        {
            this.HasMaximizeButton = true;
            this.HasMinimizeButton = true;
            this.DataContext = newIssueViewModel;
            this.InitializeComponent();
        }
    }
}
