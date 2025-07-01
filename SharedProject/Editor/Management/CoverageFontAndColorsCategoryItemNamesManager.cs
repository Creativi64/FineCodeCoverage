using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Options.Base;
using FineCodeCoverage.Options.EditorCoverageColouring;

namespace FineCodeCoverage.Editor.Management
{
    [Export(typeof(ICoverageFontAndColorsCategoryItemNames))]
    [Export(typeof(ICoverageFontAndColorsCategoryItemNamesManager))]
    internal sealed class CoverageFontAndColorsCategoryItemNamesManager : ICoverageFontAndColorsCategoryItemNames, ICoverageFontAndColorsCategoryItemNamesManager
    {
        private readonly Guid _editorTextMarkerFontAndColorCategory = new Guid("FF349800-EA43-46C1-8C98-878E78F46501");
        private readonly Guid _editorMEFCategory = new Guid("75A05685-00A8-4DED-BAE5-E7A50BFA929A");
        private readonly bool _hasCoverageMarkers;
        private readonly IOptionsProvider<EditorCoverageColouringOptions> _editorCoverageColouringOptionsProvider;
        private FCCEditorFormatDefinitionNames _fCCEditorFormatDefinitionNames;
        private bool _usingEnterprise = false;
        private bool _initialized = false;

        public event EventHandler Changed;

        [ImportingConstructor]
        public CoverageFontAndColorsCategoryItemNamesManager(
            IVsHasCoverageMarkersLogic vsHasCoverageMarkersLogic,
            IOptionsProvider<EditorCoverageColouringOptions> editorCoverageColouringOptionsProvider)
        {
            editorCoverageColouringOptionsProvider.OptionsChanged += EditorCoverageColouringOptionsProvider_OptionsChanged;
            _hasCoverageMarkers = vsHasCoverageMarkersLogic.HasCoverageMarkers();
            _editorCoverageColouringOptionsProvider = editorCoverageColouringOptionsProvider;
        }

        private void EditorCoverageColouringOptionsProvider_OptionsChanged(EditorCoverageColouringOptions editorCoverageColouringOptions)
        {
            if (!_initialized)
            {
                return;
            }

            ReactToAppOptionsChanging(editorCoverageColouringOptions);
        }

        private void ReactToAppOptionsChanging(EditorCoverageColouringOptions editorCoverageColouringOptions)
        {
            bool preUsingEnterprise = _usingEnterprise;
            Set(() => editorCoverageColouringOptions.UseEnterpriseFontsAndColors);
            if (_usingEnterprise == preUsingEnterprise)
            {
                return;
            }

            Changed?.Invoke(this, EventArgs.Empty);
        }

        public void Initialize(FCCEditorFormatDefinitionNames fCCEditorFormatDefinitionNames)
        {
            _fCCEditorFormatDefinitionNames = fCCEditorFormatDefinitionNames;
            Set();
            _initialized = true;
        }

        private void Set() => Set(() => _editorCoverageColouringOptionsProvider.Get().UseEnterpriseFontsAndColors);

        private void Set(Func<bool> getUseEnterprise)
        {
            if (!_hasCoverageMarkers)
            {
                SetMarkersFromFCC();
            }
            else
            {
                SetPossiblyEnterprise(getUseEnterprise());
            }

            SetFCCOnly();
        }

        private void SetPossiblyEnterprise(bool useEnterprise)
        {
            _usingEnterprise = useEnterprise;
            if (useEnterprise)
            {
                SetMarkersFromEnterprise();
            }
            else
            {
                SetMarkersFromFCC();
            }
        }

        private void SetFCCOnly()
        {
            NewLines = CreateMef(_fCCEditorFormatDefinitionNames.NewLines);
            Dirty = CreateMef(_fCCEditorFormatDefinitionNames.Dirty);
            NotIncluded = CreateMef(_fCCEditorFormatDefinitionNames.NotIncluded);
        }

        private void SetMarkersFromFCC()
        {
            Covered = CreateMef(_fCCEditorFormatDefinitionNames.Covered);
            NotCovered = CreateMef(_fCCEditorFormatDefinitionNames.NotCovered);
            PartiallyCovered = CreateMef(_fCCEditorFormatDefinitionNames.PartiallyCovered);
        }

        private void SetMarkersFromEnterprise()
        {
            Covered = CreateEnterprise(MarkerTypeNames.Covered);
            NotCovered = CreateEnterprise(MarkerTypeNames.NotCovered);
            PartiallyCovered = CreateEnterprise(MarkerTypeNames.PartiallyCovered);
        }

        private FontAndColorsCategoryItemName CreateMef(string itemName)
            => new FontAndColorsCategoryItemName(itemName, _editorMEFCategory);

        private FontAndColorsCategoryItemName CreateEnterprise(string itemName)
            => new FontAndColorsCategoryItemName(itemName, _editorTextMarkerFontAndColorCategory);

        public FontAndColorsCategoryItemName Covered { get; private set; }

        public FontAndColorsCategoryItemName NotCovered { get; private set; }

        public FontAndColorsCategoryItemName PartiallyCovered { get; private set; }

        public FontAndColorsCategoryItemName NewLines { get; private set; }

        public FontAndColorsCategoryItemName Dirty { get; private set; }

        public FontAndColorsCategoryItemName NotIncluded { get; private set; }

        public ICoverageFontAndColorsCategoryItemNames CategoryItemNames => this;
    }
}
