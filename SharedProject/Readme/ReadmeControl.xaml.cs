using System.Windows.Controls;

namespace FineCodeCoverage.Readme
{
    /// <summary>
    /// readme flow document.
    /// </summary>
    public partial class ReadmeControl : UserControl
    {
        public ReadmeControl() => InitializeComponent();

        public FindRestylingFlowDocumentReader FlowDocumentReader => findRestylingFlowDocumentReader;
    }
}
