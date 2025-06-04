using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media;
using FineCodeCoverage.Core.Initialization;
using FineCodeCoverage.Editor.DynamicCoverage;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace FineCodeCoverage.Editor.Management
{
    [Export(typeof(IInitializable))]
    internal class CoverageColoursManager : IInitializable
    {
        private readonly ICoverageClassificationColourService _coverageClassificationColourService;
        private readonly IFontAndColorsInfosProvider _fontAndColorsInfosProvider;
        private readonly IEditorFormatMapTextSpecificListener _editorFormatMapTextSpecificListener;
        private readonly ITextFormattingRunPropertiesFactory _textFormattingRunPropertiesFactory;

        #region format definitions
        private const string PartiallyCoveredEditorFormatDefinitionName = "Coverage Partially Touched Area FCC";
        private const string NotCoveredEditorFormatDefinitionName = "Coverage Not Touched Area FCC";
        private const string CoveredEditorFormatDefinitionName = "Coverage Touched Area FCC";
        private const string NewLinesEditorFormatDefinitionName = "Coverage New Lines Area FCC";
        private const string DirtyEditorFormatDefinitionName = "Coverage Dirty Area FCC";
        private const string NotIncludedEditorFormatDefintionName = "Coverage Not Included Area FCC";

        [Export]
        [Name(NotIncludedEditorFormatDefintionName)]
        [UserVisible(true)]
        public EditorFormatDefinition NotIncludedEditorFormatDefinition { get; } = new ColoursClassificationFormatDefinition(Colors.Black, Colors.LightPink);

        [Export]
        [Name(NewLinesEditorFormatDefinitionName)]
        [UserVisible(true)]
        public EditorFormatDefinition NewLinesEditorFormatDefinition { get; } = new ColoursClassificationFormatDefinition(Colors.Black, Colors.Yellow);

        [Export]
        [Name(DirtyEditorFormatDefinitionName)]
        [UserVisible(true)]
        public EditorFormatDefinition DirtyEditorFormatDefinition { get; } = new ColoursClassificationFormatDefinition(Colors.White, Colors.Brown);

        [Export]
        [Name(CoveredEditorFormatDefinitionName)]
        [UserVisible(true)]
        public EditorFormatDefinition CoveredEditorFormatDefinition { get; } = new ColoursClassificationFormatDefinition(Colors.Black, Color.FromRgb(16, 135, 24));

        [Export]
        [Name(NotCoveredEditorFormatDefinitionName)]
        [UserVisible(true)]
        public EditorFormatDefinition NotCoveredEditorFormatDefinition { get; } = new ColoursClassificationFormatDefinition(Colors.White, Colors.Red);

        [Export]
        [Name(PartiallyCoveredEditorFormatDefinitionName)]
        [UserVisible(true)]
        public EditorFormatDefinition PartiallyCoveredEditorFormatDefinition { get; } = new ColoursClassificationFormatDefinition(Colors.Black, Color.FromRgb(255, 165, 0));

        #endregion

        [ImportingConstructor]
        public CoverageColoursManager(
            ICoverageClassificationColourService coverageClassificationColourService,
            IFontAndColorsInfosProvider fontAndColorsInfosProvider,
            IEditorFormatMapTextSpecificListener editorFormatMapTextSpecificListener,
            ITextFormattingRunPropertiesFactory textFormattingRunPropertiesFactory,
            IDelayedMainThreadInvocation delayedMainThreadInvocation,
            ICoverageFontAndColorsCategoryItemNamesManager coverageFontAndColorsCategoryItemNamesManager
        )
        {
            _coverageClassificationColourService = coverageClassificationColourService;
            _fontAndColorsInfosProvider = fontAndColorsInfosProvider;
            _editorFormatMapTextSpecificListener = editorFormatMapTextSpecificListener;
            _textFormattingRunPropertiesFactory = textFormattingRunPropertiesFactory;

            coverageFontAndColorsCategoryItemNamesManager.Initialize(
                new FCCEditorFormatDefinitionNames(
                    CoveredEditorFormatDefinitionName,
                    NotCoveredEditorFormatDefinitionName,
                    PartiallyCoveredEditorFormatDefinitionName,
                    NewLinesEditorFormatDefinitionName,
                    DirtyEditorFormatDefinitionName,
                    NotIncludedEditorFormatDefintionName
            ));
            coverageFontAndColorsCategoryItemNamesManager.Changed += (sender, args) => Changed();
            fontAndColorsInfosProvider.CoverageFontAndColorsCategoryItemNames = coverageFontAndColorsCategoryItemNamesManager.CategoryItemNames;

            _editorFormatMapTextSpecificListener.ListenFor(
                new List<string> {
                    MarkerTypeNames.Covered,
                    MarkerTypeNames.NotCovered,
                    MarkerTypeNames.PartiallyCovered,
                    CoveredEditorFormatDefinitionName,
                    NotCoveredEditorFormatDefinitionName,
                    PartiallyCoveredEditorFormatDefinitionName,

                    NewLinesEditorFormatDefinitionName,
                    DirtyEditorFormatDefinitionName,
                    NotIncludedEditorFormatDefintionName
                },
                () => Changed());

            delayedMainThreadInvocation.DelayedInvoke(InitializeColours);
        }

        private void InitializeColours()
        {
            Dictionary<DynamicCoverageType, IFontAndColorsInfo> coverageColors = _fontAndColorsInfosProvider.GetFontAndColorsInfos();
            SetClassificationTypeColoursIfChanged(coverageColors);
        }

        private void Changed()
        {
            Dictionary<DynamicCoverageType, IFontAndColorsInfo> changedColours = _fontAndColorsInfosProvider.GetChangedFontAndColorsInfos();
            SetClassificationTypeColoursIfChanged(changedColours);
        }

        private void SetClassificationTypeColoursIfChanged(Dictionary<DynamicCoverageType, IFontAndColorsInfo> changes)
        {
            if (changes.Count == 0)
            {
                return;
            }

            _editorFormatMapTextSpecificListener.PauseListeningWhenExecuting(
                () => SetClassificationTypeColours(changes)
            );
        }

        private void SetClassificationTypeColours(Dictionary<DynamicCoverageType, IFontAndColorsInfo> changes)
        {
            IEnumerable<CoverageTypeColour> coverageTypeColours = changes.Select(
                change => new CoverageTypeColour(change.Key, _textFormattingRunPropertiesFactory.Create(change.Value))
            );
            _coverageClassificationColourService.SetCoverageColours(coverageTypeColours);
        }
    }
}
