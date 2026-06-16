using System.Windows.Controls;

namespace EnhancedFlowDocumentControls.Management
{
    internal interface IFlowControlReflector
    {
        bool CanShowFindToolBar(object flowControl);

        Decorator GetFindToolBarHost(object flowControl);

        void SetFindToolBarHost(object flowControl, Decorator value);
    }
}
