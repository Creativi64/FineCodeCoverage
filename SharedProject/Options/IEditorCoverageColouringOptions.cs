using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Options
{
    internal class EditorCoverageColouringOptions : IEditorCoverageColouringOptions
    {
        private const string editorColouringControlCategory = "Editor Colouring Control";
        private const string overviewMarginCategory = "Editor Colouring Overview Margin";
        private const string glyphMarginCategory = "Editor Colouring Glyph Margin";
        private const string lineHighlightingCategory = "Editor Colouring Line Highlighting";

        #region editorColouringControlCategory
        [Category(editorColouringControlCategory)]
        [Description("Set to true to limit coverage lines in .razor file to those in generated source ( when available)")]
        public bool BlazorCoverageLinesFromGeneratedSource { get; set; }

        [Category(editorColouringControlCategory)]
        [Description("Set to false to disable all editor coverage indicators")]
        //[DisplayName("Show Editor Coverage")]
        public bool ShowEditorCoverage { get; set; }

        [Category(editorColouringControlCategory)]
        [Description("Set to false to use FCC Fonts And Colors items")]
        public bool UseEnterpriseFontsAndColors { get; set; }

        [Category(editorColouringControlCategory)]
        [Description("Set to Off, or Set to DoNotUseRoslynWhenTextChanges if there is a performance issue")]
        public EditorCoverageColouringMode EditorCoverageColouringMode { get; set; }
        #endregion

        #region overview margin
        [Category(overviewMarginCategory)]
        [Description("Set to false to prevent coverage marks in the overview margin")]
        //[DisplayName("Show Overview Margin Coverage")]
        public bool ShowCoverageInOverviewMargin { get; set; }

        [Category(overviewMarginCategory)]
        [Description("Set to false to prevent covered marks in the overview margin")]
        //[DisplayName("Show Overview Margin Covered")]
        public bool ShowCoveredInOverviewMargin { get; set; }

        [Category(overviewMarginCategory)]
        [Description("Set to false to prevent uncovered marks in the overview margin")]
        //[DisplayName("Show Overview Margin Uncovered")]
        public bool ShowUncoveredInOverviewMargin { get; set; }

        [Category(overviewMarginCategory)]
        [Description("Set to false to prevent partially covered marks in the overview margin")]
        //[DisplayName("Show Overview Margin Partially Covered")]
        public bool ShowPartiallyCoveredInOverviewMargin { get; set; }

        [Category(overviewMarginCategory)]
        [Description("Set to true to show dirty marks in the overview margin")]
        public bool ShowDirtyInOverviewMargin { get; set; }

        [Category(overviewMarginCategory)]
        [Description("Set to true to show new line marks in the overview margin")]
        public bool ShowNewInOverviewMargin { get; set; }

        [Category(overviewMarginCategory)]
        [Description("Set to true to show not included marks in the overview margin")]
        public bool ShowNotIncludedInOverviewMargin { get; set; }
        #endregion
        #region glyph margin
        [Category(glyphMarginCategory)]
        [Description("Set to false to prevent coverage marks in the glyph margin")]
        //[DisplayName("Show Glyph Margin Coverage")]
        public bool ShowCoverageInGlyphMargin { get; set; }

        [Category(glyphMarginCategory)]
        [Description("Set to false to prevent covered marks in the glyph margin")]
        //[DisplayName("Show Glyph Margin Covered")]
        public bool ShowCoveredInGlyphMargin { get; set; }

        [Category(glyphMarginCategory)]
        [Description("Set to false to prevent uncovered marks in the glyph margin")]
        //[DisplayName("Show Glyph Margin Uncovered")]
        public bool ShowUncoveredInGlyphMargin { get; set; }

        [Category(glyphMarginCategory)]
        [Description("Set to false to prevent partially covered marks in the glyph margin")]
        //[DisplayName("Show Glyph Margin Partially Covered")]
        public bool ShowPartiallyCoveredInGlyphMargin { get; set; }

        [Category(glyphMarginCategory)]
        [Description("Set to true to show dirty marks in the glyph margin")]
        public bool ShowDirtyInGlyphMargin { get; set; }

        [Category(glyphMarginCategory)]
        [Description("Set to true to show new line marks in the glyph margin")]
        public bool ShowNewInGlyphMargin { get; set; }

        [Category(glyphMarginCategory)]
        [Description("Set to true to show not included marks in the glyph margin")]
        public bool ShowNotIncludedInGlyphMargin { get; set; }
        #endregion
        #region line highlighting
        [Category(lineHighlightingCategory)]
        [Description("Set to true to allow coverage line highlighting")]
        //[DisplayName("Show Line Highlighting Coverage")]
        public bool ShowLineCoverageHighlighting { get; set; }

        [Category(lineHighlightingCategory)]
        [Description("Set to false to prevent covered line highlighting")]
        //[DisplayName("Show Line Highlighting Covered")]
        public bool ShowLineCoveredHighlighting { get; set; }

        [Category(lineHighlightingCategory)]
        [Description("Set to false to prevent uncovered line highlighting")]
        //[DisplayName("Show Line Highlighting Uncovered")]
        public bool ShowLineUncoveredHighlighting { get; set; }

        [Category(lineHighlightingCategory)]
        [Description("Set to false to prevent partially covered line highlighting")]
        //[DisplayName("Show Line Highlighting Partially Covered")]
        public bool ShowLinePartiallyCoveredHighlighting { get; set; }

        [Category(lineHighlightingCategory)]
        [Description("Set to true to show dirty line highlighting")]
        public bool ShowLineDirtyHighlighting { get; set; }
        [Category(lineHighlightingCategory)]
        [Description("Set to true to show new line highlighting")]
        public bool ShowLineNewHighlighting { get; set; }

        [Category(lineHighlightingCategory)]
        [Description("Set to true to show not included highlighting")]
        public bool ShowLineNotIncludedHighlighting { get; set; }

        #endregion
    }

    internal class EditorCoverageColouringOptionsPage : DialogPageBase<EditorCoverageColouringOptions> { }

    [Export(typeof(IDefaultOptionsSetter<EditorCoverageColouringOptions>))]
    internal class EditorCoverageColouringOptionsDefaults : IDefaultOptionsSetter<EditorCoverageColouringOptions>
    {
        public void Set(EditorCoverageColouringOptions options)
        {
            options.ShowEditorCoverage = true;
            options.ShowCoverageInOverviewMargin = true;
            options.ShowCoveredInOverviewMargin = true;
            options.ShowPartiallyCoveredInOverviewMargin = true;
            options.ShowUncoveredInOverviewMargin = true;

            options.ShowCoverageInGlyphMargin = true;
            options.ShowCoveredInGlyphMargin = true;
            options.ShowPartiallyCoveredInGlyphMargin = true;
            options.ShowUncoveredInGlyphMargin = true;

            options.ShowLineCoveredHighlighting = true;
            options.ShowLinePartiallyCoveredHighlighting = true;
            options.ShowLineUncoveredHighlighting = true;

            options.UseEnterpriseFontsAndColors = true;
        }
    }

    internal interface IEditorCoverageColouringOptionsProvider
    {
        // The argument is the same from Get
        event Action<EditorCoverageColouringOptions> OptionsChanged;
        // returns the same instance each time
        EditorCoverageColouringOptions Get();
    }

    [Export(typeof(IEditorCoverageColouringOptionsProvider))]
    [Export(typeof(IRequireDialogPageInstantiator))]
    [Export(typeof(IDialogPageOptionsProvider<EditorCoverageColouringOptions>))]
    [Export(typeof(IProfileOptionsProvider))]
    internal class EditorCoverageColouringOptionsProvider : OptionsProviderBase<EditorCoverageColouringOptions>, IEditorCoverageColouringOptionsProvider
    {
        [ImportingConstructor]
        public EditorCoverageColouringOptionsProvider(
            ILogger logger,
            IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider,
            IJsonConvertService jsonConvertService,
            IDefaultOptionsSetter<EditorCoverageColouringOptions> defaultOptionsSetter
        ) : base(logger, writableUserSettingsStoreProvider, jsonConvertService, defaultOptionsSetter) { }
    }
}
