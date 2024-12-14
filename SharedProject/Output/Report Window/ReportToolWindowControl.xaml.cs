using System.Windows.Controls;

namespace FineCodeCoverage.Output
{
	/// <summary>
	/// Interaction logic for OutputToolWindowControl.
	/// </summary>
	internal partial class ReportToolWindowControl : 
		UserControl
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportToolWindowControl"/> class.
        /// </summary>
        public ReportToolWindowControl(ReportViewModel reportViewModel)
		{
            this.DataContext = reportViewModel;
            this.InitializeComponent();
		}
    }
}