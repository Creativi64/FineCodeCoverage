using System.Windows.Input;

namespace EnhancedFlowDocumentControls.ViewModel
{
    public interface IFindToolBarViewModel : IFindToolBarSettings
    {
        bool AllowSearchingWhenEmptyText { get; set; }

        bool IsSearchDown { get; }

        ICommand NextCommand { get; }

        ICommand PreviousCommand { get; }

        object OriginalDataContext { get; }
    }
}
