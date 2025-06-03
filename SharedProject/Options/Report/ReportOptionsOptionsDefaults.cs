using System.ComponentModel.Composition;

namespace FineCodeCoverage.Options
{
    [Export(typeof(IDefaultOptionsSetter<ReportOptions>))]
    internal class ReportOptionsOptionsDefaults : IDefaultOptionsSetter<ReportOptions>
    {
        public void Set(ReportOptions options)
        {
            options.Hide0Coverable = true;
            options.CoveragePercentageIsThemed = true;
            options.CoveragePercentageCoveredIsLeft = true;
            options.CoveragePercentageUseSolidBrush = true;
            options.CoveragePercentageShowTooltip = true;
            options.HeaderUseTabularSharedColors = true;
            options.ShowIcons = true;
            options.IconSize = 16;
        }
    }
}