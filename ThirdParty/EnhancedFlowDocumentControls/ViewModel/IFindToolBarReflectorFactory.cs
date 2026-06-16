using System.Windows.Controls;

namespace EnhancedFlowDocumentControls.ViewModel
{
    internal interface IFindToolBarReflectorFactory
    {
        IFindToolBarReflector CreateReflector(ToolBar findToolBar);
    }
}
