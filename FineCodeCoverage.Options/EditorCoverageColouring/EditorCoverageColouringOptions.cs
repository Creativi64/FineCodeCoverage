using System.ComponentModel;

namespace FineCodeCoverage.Options.EditorCoverageColouring
{
    /*
        Note that option properties must not be renamed
    */
    public sealed class EditorCoverageColouringOptions
    {
        private const string ControlCategory = "Control";
        private const string OverviewMarginCategory = "Overview Margin";
        private const string GlyphMarginCategory = "Glyph Margin";
        private const string LineHighlightingCategory = "Line Highlighting";

        #region Control Category
        [Category(ControlCategory)]
        [Description("Set to true to limit coverage lines in .razor file to those in generated source ( when available)")]
        [DisplayName("Blazor Coverage Lines From Generated Source")]
        public bool BlazorCoverageLinesFromGeneratedSource { get; set; }

        [Category(ControlCategory)]
        [Description("Set to false to disable all editor coverage indicators")]
        [DisplayName("Show Editor Coverage")]
        public bool ShowEditorCoverage { get; set; }

        [Category(ControlCategory)]
        [Description("Set to false to use FCC Fonts And Colors items")]
        [DisplayName("Use Enterprise Fonts And Colors")]
        public bool UseEnterpriseFontsAndColors { get; set; }

        [Category(ControlCategory)]
        [Description("Set to Off, or Set to DoNotUseRoslynWhenTextChanges if there is a performance issue")]
        [DisplayName("Mode")]
        public EditorCoverageColouringMode EditorCoverageColouringMode { get; set; }
        #endregion

        #region Overview Margin
        [Category(OverviewMarginCategory)]
        [Description("Set to false to prevent coverage marks in the overview margin")]
        [DisplayName("Show")]
        public bool ShowCoverageInOverviewMargin { get; set; }

        [Category(OverviewMarginCategory)]
        [Description("Set to false to prevent covered marks in the overview margin")]
        [DisplayName("Show Covered")]
        public bool ShowCoveredInOverviewMargin { get; set; }

        [Category(OverviewMarginCategory)]
        [Description("Set to false to prevent uncovered marks in the overview margin")]
        [DisplayName("Show Uncovered")]
        public bool ShowUncoveredInOverviewMargin { get; set; }

        [Category(OverviewMarginCategory)]
        [Description("Set to false to prevent partially covered marks in the overview margin")]
        [DisplayName("Show Partially Covered")]
        public bool ShowPartiallyCoveredInOverviewMargin { get; set; }

        [Category(OverviewMarginCategory)]
        [Description("Set to true to show dirty marks in the overview margin")]
        [DisplayName("Show Dirty")]
        public bool ShowDirtyInOverviewMargin { get; set; }

        [Category(OverviewMarginCategory)]
        [Description("Set to true to show new line marks in the overview margin")]
        [DisplayName("Show New")]
        public bool ShowNewInOverviewMargin { get; set; }

        [Category(OverviewMarginCategory)]
        [Description("Set to true to show not included marks in the overview margin")]
        [DisplayName("Show Not Included")]
        public bool ShowNotIncludedInOverviewMargin { get; set; }
        #endregion

        #region Glyph Margin
        [Category(GlyphMarginCategory)]
        [Description("Set to false to prevent coverage marks in the glyph margin")]
        [DisplayName("Show")]
        public bool ShowCoverageInGlyphMargin { get; set; }

        [Category(GlyphMarginCategory)]
        [Description("Set to false to prevent covered marks in the glyph margin")]
        [DisplayName("Show Covered")]
        public bool ShowCoveredInGlyphMargin { get; set; }

        [Category(GlyphMarginCategory)]
        [Description("Set to false to prevent uncovered marks in the glyph margin")]
        [DisplayName("Show Uncovered")]
        public bool ShowUncoveredInGlyphMargin { get; set; }

        [Category(GlyphMarginCategory)]
        [Description("Set to false to prevent partially covered marks in the glyph margin")]
        [DisplayName("Show Partially Covered")]
        public bool ShowPartiallyCoveredInGlyphMargin { get; set; }

        [Category(GlyphMarginCategory)]
        [Description("Set to true to show dirty marks in the glyph margin")]
        [DisplayName("Show Dirty")]
        public bool ShowDirtyInGlyphMargin { get; set; }

        [Category(GlyphMarginCategory)]
        [Description("Set to true to show new line marks in the glyph margin")]
        [DisplayName("Show New")]
        public bool ShowNewInGlyphMargin { get; set; }

        [Category(GlyphMarginCategory)]
        [Description("Set to true to show not included marks in the glyph margin")]
        [DisplayName("Show Not Included")]
        public bool ShowNotIncludedInGlyphMargin { get; set; }
        #endregion

        #region Line Highlighting
        [Category(LineHighlightingCategory)]
        [Description("Set to true to allow coverage line highlighting")]
        [DisplayName("Show")]
        public bool ShowLineCoverageHighlighting { get; set; }

        [Category(LineHighlightingCategory)]
        [Description("Set to false to prevent covered line highlighting")]
        [DisplayName("Show Covered")]
        public bool ShowLineCoveredHighlighting { get; set; }

        [Category(LineHighlightingCategory)]
        [Description("Set to false to prevent uncovered line highlighting")]
        [DisplayName("Show Uncovered")]
        public bool ShowLineUncoveredHighlighting { get; set; }

        [Category(LineHighlightingCategory)]
        [Description("Set to false to prevent partially covered line highlighting")]
        [DisplayName("Show Partially Covered")]
        public bool ShowLinePartiallyCoveredHighlighting { get; set; }

        [Category(LineHighlightingCategory)]
        [Description("Set to true to show dirty line highlighting")]
        [DisplayName("Show Dirty")]
        public bool ShowLineDirtyHighlighting { get; set; }

        [Category(LineHighlightingCategory)]
        [Description("Set to true to show new line highlighting")]
        [DisplayName("Show New")]
        public bool ShowLineNewHighlighting { get; set; }

        [Category(LineHighlightingCategory)]
        [Description("Set to true to show not included highlighting")]
        [DisplayName("Show Not Included")]
        public bool ShowLineNotIncludedHighlighting { get; set; }
        #endregion
    }
}
