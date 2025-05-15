using System.ComponentModel.Composition;

namespace FineCodeCoverage.Options
{

    [Export(typeof(IDefaultOptionsSetter<AppOptions>))]
    internal class AppOptionsDefaults : IDefaultOptionsSetter<AppOptions>
    {
        public void Set(AppOptions appOptions)
        {
            appOptions.ThresholdForCrapScore = 15;
            appOptions.ThresholdForNPathComplexity = 200;
            appOptions.ThresholdForCyclomaticComplexity = 30;
            appOptions.RunMsCodeCoverage = RunMsCodeCoverage.Yes;
            appOptions.RunSettingsOnly = true;
            appOptions.RunWhenTestsFail = true;
            appOptions.ExcludeByAttribute = new[] { "GeneratedCode" };
            appOptions.IncludeTestAssembly = true;
            appOptions.ExcludeByFile = new[] { "**/Migrations/*" };
            appOptions.Enabled = true;
            appOptions.DisabledNoCoverage = true;
            //appOptions.ShowEditorCoverage = true;
            //appOptions.ShowCoverageInOverviewMargin = true;
            //appOptions.ShowCoveredInOverviewMargin = true;
            //appOptions.ShowPartiallyCoveredInOverviewMargin = true;
            //appOptions.ShowUncoveredInOverviewMargin = true;

            //appOptions.ShowCoverageInGlyphMargin = true;
            //appOptions.ShowCoveredInGlyphMargin = true;
            //appOptions.ShowPartiallyCoveredInGlyphMargin = true;
            //appOptions.ShowUncoveredInGlyphMargin = true;

            //appOptions.ShowLineCoveredHighlighting = true;
            //appOptions.ShowLinePartiallyCoveredHighlighting = true;
            //appOptions.ShowLineUncoveredHighlighting = true;

            //appOptions.UseEnterpriseFontsAndColors = true;

            appOptions.Hide0Coverable = true;

            appOptions.CoveragePercentageIsThemed = true;
            appOptions.CoveragePercentageCoveredIsLeft = true;
            appOptions.CoveragePercentageUseSolidBrush = true;
            appOptions.CoveragePercentageShowTooltip = true;
            appOptions.HeaderUseTabularSharedColors = true;
            appOptions.ShowIcons = true;
            appOptions.IconSize = 16;
        }
    }

}
