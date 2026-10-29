using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FineCodeCoverage.Editor.DynamicCoverage.Common;
using FineCodeCoverage.Initialization;
using static FineCodeCoverage.Editor.Management.CoverageEditorFormatDefinitions;

namespace FineCodeCoverage.Editor.Management
{
    [Export(typeof(IInitializable))]
    internal sealed class CoverageColoursManager : IInitializable
    {
        private readonly ICoverageClassificationColourService _coverageClassificationColourService;
        private readonly IFontAndColorsInfosProvider _fontAndColorsInfosProvider;
        private readonly IEditorFormatMapTextSpecificListener _editorFormatMapTextSpecificListener;
        private readonly ITextFormattingRunPropertiesFactory _textFormattingRunPropertiesFactory;

        [ImportingConstructor]
        public CoverageColoursManager(
            ICoverageClassificationColourService coverageClassificationColourService,
            IFontAndColorsInfosProvider fontAndColorsInfosProvider,
            IEditorFormatMapTextSpecificListener editorFormatMapTextSpecificListener,
            ITextFormattingRunPropertiesFactory textFormattingRunPropertiesFactory,
            IDelayedMainThreadInvocation delayedMainThreadInvocation,
            ICoverageFontAndColorsCategoryItemNamesManager coverageFontAndColorsCategoryItemNamesManager)
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
                    NotIncludedEditorFormatDefinitionName));
            coverageFontAndColorsCategoryItemNamesManager.Changed += (sender, args) => Changed();
            fontAndColorsInfosProvider.CoverageFontAndColorsCategoryItemNames = coverageFontAndColorsCategoryItemNamesManager.CategoryItemNames;

            _editorFormatMapTextSpecificListener.ListenFor(
                new List<string>
                {
                    MarkerTypeNames.Covered,
                    MarkerTypeNames.NotCovered,
                    MarkerTypeNames.PartiallyCovered,
                    CoveredEditorFormatDefinitionName,
                    NotCoveredEditorFormatDefinitionName,
                    PartiallyCoveredEditorFormatDefinitionName,

                    NewLinesEditorFormatDefinitionName,
                    DirtyEditorFormatDefinitionName,
                    NotIncludedEditorFormatDefinitionName,
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
                () => SetClassificationTypeColours(changes));
        }

        private void SetClassificationTypeColours(Dictionary<DynamicCoverageType, IFontAndColorsInfo> changes)
        {
            IEnumerable<CoverageTypeColour> coverageTypeColours = changes.Select(
                change => new CoverageTypeColour(change.Key, _textFormattingRunPropertiesFactory.Create(change.Value)));
            _coverageClassificationColourService.SetCoverageColours(coverageTypeColours);
        }
    }
}
