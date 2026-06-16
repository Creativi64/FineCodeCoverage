using System.Windows.Controls;

namespace EnhancedFlowDocumentControls.ViewModel
{
    internal sealed class FindToolBarReflectorFactory : IFindToolBarReflectorFactory
    {
        public IFindToolBarReflector CreateReflector(ToolBar findToolBar) => new FindToolBarReflector(findToolBar);
    }
}
