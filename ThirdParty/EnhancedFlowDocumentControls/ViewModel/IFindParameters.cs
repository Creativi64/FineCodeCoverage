namespace EnhancedFlowDocumentControls.ViewModel
{
    internal interface IFindParameters
    {
        IFindParameter<string> FindText { get; }

        IFindParameter<bool> IsSearchUp { get; }

        IFindParameter<bool> MatchAlefHamza { get; }

        IFindParameter<bool> MatchCase { get; }

        IFindParameter<bool> MatchDiacritic { get; }

        IFindParameter<bool> MatchKashida { get; }

        IFindParameter<bool> MatchWholeWord { get; }
    }
}
