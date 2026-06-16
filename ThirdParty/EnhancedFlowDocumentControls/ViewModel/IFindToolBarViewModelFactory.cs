using System.Windows;
using System.Windows.Controls;

namespace EnhancedFlowDocumentControls.ViewModel
{
    internal interface IFindToolBarViewModelFactory
    {
        IFindableToolBarViewModel Create(ToolBar findToolBar, FrameworkElement originalDataContextElement);
    }
}
