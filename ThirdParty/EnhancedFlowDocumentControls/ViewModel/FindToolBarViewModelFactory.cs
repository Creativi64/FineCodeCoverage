using System.Windows;
using System.Windows.Controls;

namespace EnhancedFlowDocumentControls.ViewModel
{
    internal sealed class FindToolBarViewModelFactory : IFindToolBarViewModelFactory
    {
        public IFindableToolBarViewModel Create(ToolBar findToolBar, FrameworkElement originalDataContextElement)
            => new FindToolBarViewModel(new FindToolBarFinder(findToolBar), originalDataContextElement);
    }
}
