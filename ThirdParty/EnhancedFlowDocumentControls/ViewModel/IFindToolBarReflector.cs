using System.Windows.Controls;

namespace EnhancedFlowDocumentControls.ViewModel
{
    internal interface IFindToolBarReflector
    {
        MenuItem GetMatchWholeWordMenuItem();

        MenuItem GetMatchCaseMenuItem();

        MenuItem GetMatchDiacriticMenuItem();

        MenuItem GetMatchKashidaMenuItem();

        MenuItem GetMatchAlefHamzaMenuItem();

        void SetSearchUp(bool isSearchUp);

        TextBox GetFindTextBox();

        void InvokeFind();
    }
}
