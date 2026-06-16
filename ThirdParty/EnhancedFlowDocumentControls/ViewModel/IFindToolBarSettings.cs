namespace EnhancedFlowDocumentControls.ViewModel
{
    public interface IFindToolBarSettings
    {
        string FindText { get; set; }

        bool IsSearchUp { get; }

        bool MatchAlefHamza { get; set; }

        bool MatchCase { get; set; }

        bool MatchDiacritic { get; set; }

        bool MatchKashida { get; set; }

        bool MatchWholeWord { get; set; }
    }
}
