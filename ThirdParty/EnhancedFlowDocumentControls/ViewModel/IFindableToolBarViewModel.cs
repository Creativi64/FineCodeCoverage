namespace EnhancedFlowDocumentControls.ViewModel
{
    internal interface IFindableToolBarViewModel : IFindToolBarViewModel
    {
        void ApplySettings(IFindToolBarSettings retainedSettings);

        void Find();

        void Find(bool searchUp);
    }
}
