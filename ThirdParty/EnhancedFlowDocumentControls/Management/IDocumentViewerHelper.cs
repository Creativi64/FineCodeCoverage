using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EnhancedFlowDocumentControls.Management
{
    internal interface IDocumentViewerHelper
    {
        void KeyDownHelper(KeyEventArgs e, DependencyObject findToolBarHost);

        void ToggleFindToolBarHost(Decorator host, bool showToolBar);
    }
}
