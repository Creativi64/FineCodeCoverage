using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Options;

namespace FineCodeCoverage.Editor.Management
{
    [Export(typeof(ICoverageFontAndColorsCategoryItemNames))]
    [Export(typeof(ICoverageFontAndColorsCategoryItemNamesManager))]
    internal class CoverageFontAndColorsCategoryItemNamesManager : ICoverageFontAndColorsCategoryItemNames, ICoverageFontAndColorsCategoryItemNamesManager
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
            IOptionsProvider<EditorCoverageColouringOptions> editorCoverageColouringOptionsProvider
        )
        {
            editorCoverageColouringOptionsProvider.OptionsChanged += this.EditorCoverageColouringOptionsProvider_OptionsChanged;
            this._hasCoverageMarkers = vsHasCoverageMarkersLogic.HasCoverageMarkers();
            this._editorCoverageColouringOptionsProvider = editorCoverageColouringOptionsProvider;
        }

        private void EditorCoverageColouringOptionsProvider_OptionsChanged(EditorCoverageColouringOptions editorCoverageColouringOptions)
        {
            if (this._initialized)
            {
                this.ReactToAppOptionsChanging(editorCoverageColouringOptions);
            }
        }

        private void ReactToAppOptionsChanging(EditorCoverageColouringOptions editorCoverageColouringOptions)
        {
            bool preUsingEnterprise = this._usingEnterprise;
            this.Set(() => editorCoverageColouringOptions.UseEnterpriseFontsAndColors);
            if (this._usingEnterprise != preUsingEnterprise)
            {
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Initialize(FCCEditorFormatDefinitionNames fCCEditorFormatDefinitionNames)
        {
            this._fCCEditorFormatDefinitionNames = fCCEditorFormatDefinitionNames;
            this.Set();
            this._initialized = true;
        }

        private void Set() => this.Set(() => this._editorCoverageColouringOptionsProvider.Get().UseEnterpriseFontsAndColors);

        private void Set(Func<bool> getUseEnterprise)
        {
            if (!this._hasCoverageMarkers)
            {
                this.SetMarkersFromFCC();
            }
            else
            {

                this.SetPossiblyEnterprise(getUseEnterprise());
            }

            this.SetFCCOnly();
        }

        private void SetPossiblyEnterprise(bool useEnterprise)
        {
            this._usingEnterprise = useEnterprise;
            if (useEnterprise)
            {
                this.SetMarkersFromEnterprise();
            }
            else
            {
                this.SetMarkersFromFCC();
            }
        }

        private void SetFCCOnly()
        {
            this.NewLines = this.CreateMef(this._fCCEditorFormatDefinitionNames.NewLines);
            this.Dirty = this.CreateMef(this._fCCEditorFormatDefinitionNames.Dirty);
            this.NotIncluded = this.CreateMef(this._fCCEditorFormatDefinitionNames.NotIncluded);
        }

        private void SetMarkersFromFCC()
        {
            this.Covered = this.CreateMef(this._fCCEditorFormatDefinitionNames.Covered);
            this.NotCovered = this.CreateMef(this._fCCEditorFormatDefinitionNames.NotCovered);
            this.PartiallyCovered = this.CreateMef(this._fCCEditorFormatDefinitionNames.PartiallyCovered);
        }

        private void SetMarkersFromEnterprise()
        {
            this.Covered = this.CreateEnterprise(MarkerTypeNames.Covered);
            this.NotCovered = this.CreateEnterprise(MarkerTypeNames.NotCovered);
            this.PartiallyCovered = this.CreateEnterprise(MarkerTypeNames.PartiallyCovered);
        }

        private FontAndColorsCategoryItemName CreateMef(string itemName)
            => new FontAndColorsCategoryItemName(itemName, this._editorMEFCategory);

        private FontAndColorsCategoryItemName CreateEnterprise(string itemName)
            => new FontAndColorsCategoryItemName(itemName, this._editorTextMarkerFontAndColorCategory);

        public FontAndColorsCategoryItemName Covered { get; private set; }
        public FontAndColorsCategoryItemName NotCovered { get; private set; }
        public FontAndColorsCategoryItemName PartiallyCovered { get; private set; }
        public FontAndColorsCategoryItemName NewLines { get; private set; }
        public FontAndColorsCategoryItemName Dirty { get; private set; }

        public FontAndColorsCategoryItemName NotIncluded { get; private set; }
        public ICoverageFontAndColorsCategoryItemNames CategoryItemNames => this;
    }
}