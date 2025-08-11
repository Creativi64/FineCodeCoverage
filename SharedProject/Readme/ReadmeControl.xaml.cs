using System.Windows.Controls;
using StylableFindFlowDocumentReader;

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
