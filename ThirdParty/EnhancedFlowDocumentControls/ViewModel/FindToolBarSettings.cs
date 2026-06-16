namespace EnhancedFlowDocumentControls.ViewModel
{
    internal sealed class FindToolBarSettings : IFindToolBarSettings
    {
        public static IFindToolBarSettings Clone(IFindToolBarSettings findToolBarSettings)
            => new FindToolBarSettings
            {
                FindText = findToolBarSettings.FindText,
                MatchAlefHamza = findToolBarSettings.MatchAlefHamza,
                MatchCase = findToolBarSettings.MatchCase,
                MatchDiacritic = findToolBarSettings.MatchDiacritic,
                MatchKashida = findToolBarSettings.MatchKashida,
                MatchWholeWord = findToolBarSettings.MatchWholeWord,
                IsSearchUp = findToolBarSettings.IsSearchUp,
            };

        private FindToolBarSettings()
        {
        }

        public string FindText { get; set; }

        public bool IsSearchUp { get; private set; }

        public bool MatchAlefHamza { get; set; }

        public bool MatchCase { get; set; }

        public bool MatchDiacritic { get; set; }

        public bool MatchKashida { get; set; }

        public bool MatchWholeWord { get; set; }
    }
}
