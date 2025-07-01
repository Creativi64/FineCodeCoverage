using System.ComponentModel.Composition;

namespace FineCodeCoverage.Options
{
    [Export(typeof(IDefaultOptionsSetter<EditorCoverageColouringOptions>))]
    internal sealed class EditorCoverageColouringOptionsDefaults : IDefaultOptionsSetter<EditorCoverageColouringOptions>
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
}
