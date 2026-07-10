using System.ComponentModel.Composition;
using FineCodeCoverage.Options.Base;

namespace FineCodeCoverage.Options.Report
{
    [Export(typeof(IDefaultOptionsSetter<ReportOptions>))]
    internal sealed class ReportOptionsOptionsDefaults : IDefaultOptionsSetter<ReportOptions>
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
