using System.Windows.Controls;

namespace FineCodeCoverage.Output
{
    /// <summary>
    /// Initialize the ReportToolWindowControl with the provided ReportViewModel.
    /// </summary>
    internal sealed partial class ReportToolWindowControl :
        UserControl
    {
        public ReportToolWindowControl(ReportViewModel reportViewModel)
        {
            DataContext = reportViewModel;
            InitializeComponent();
        }
    }
}
