using System.ComponentModel;

namespace FineCodeCoverage.Options
{
    /*
        Note that option properties must not be renamed
    */
    internal sealed class ReportOptions
    {
        private const string FiltersCategory = "Filters";
        private const string IconCategory = "Icons";
        private const string DisplayCategory = "Display";
        private const string CoverageBarCategory = "Coverage Bar";
        private const string SourceViewCategory = "Source View";

        #region filters category
        [Category(FiltersCategory)]
        [Description("Set to true to hide classes, namespaces and assemblies that are fully covered.")]
        [DisplayName("Hide Fully Covered")]
        public bool HideFullyCovered { get; set; }

        [Category(FiltersCategory)]
        [Description("Set to false to show classes, namespaces and assemblies that are not coverable.")]
        [DisplayName("Hide Not Coverable")]
        public bool Hide0Coverable { get; set; }

        [Category(FiltersCategory)]
        [Description("Set to true to hide classes, namespaces and assemblies that have 0% coverage.")]
        [DisplayName("Hide 0% Coverage")]
        public bool Hide0Coverage { get; set; }
        #endregion

        #region icons category
        [Category(IconCategory)]
        [Description("Set to false to not display tree item icons")]
        [DisplayName("Show icons")]
        public bool ShowIcons { get; set; }

        [Category(IconCategory)]
        [Description("Set the icon size")]
        [DisplayName("Icon size")]
        public int IconSize { get; set; }

        [Category(IconCategory)]
        [Description("Themed icon style")]
        [DisplayName("Themed Style")]
        public ThemedIconStyle ThemedIconStyle { get; set; }
        #endregion

        #region coverage bar category
        [Category(CoverageBarCategory)]
        [Description("Covered on the left or the right")]
        [DisplayName("Covered is left")]
        public bool CoveragePercentageCoveredIsLeft { get; set; }

        [Category(CoverageBarCategory)]
        [Description("Display covered and uncovered percentages ( Both ), Covered or UnCovered")]
        [DisplayName("Display parts")]
        public CoveragePercentageBarDisplayParts CoveragePercentageDisplayParts { get; set; }

        [Category(CoverageBarCategory)]
        [Description("Theme colours against the background.")]
        [DisplayName("Is themed")]
        public bool CoveragePercentageIsThemed { get; set; }

        [Category(CoverageBarCategory)]
        [Description("Use colours from Environment / Fonts and Colors - e.g Coverage Touched Area FCC")]
        [DisplayName("Use fonts and colors")]
        public bool CoveragePercentageUseColorsFromFontsAndColors { get; set; }

        [Category(CoverageBarCategory)]
        [Description("Set to false for dashed line brush")]
        [DisplayName("Use solid brush")]
        public bool CoveragePercentageUseSolidBrush { get; set; }

        [Category(CoverageBarCategory)]
        [Description("Use contrast color when singular display")]
        [DisplayName("Use contrast color when singular display")]
        public bool CoveragePercentageUseContrastedThemeWhenSingularDisplay { get; set; }

        [Category(CoverageBarCategory)]
        [Description("Set to between 0 and 1 as percentage of environment font size or a specific height.")]
        [DisplayName("Height in pixels or multiplier")]
        public double? CoveragePercentageHeightOrMultiplier { get; set; }

        [Category(CoverageBarCategory)]
        [Description("Set to false to hide tooltip showing contributing amounts.")]
        [DisplayName("Show tooltip")]
        public bool CoveragePercentageShowTooltip { get; set; }
        #endregion

        #region display category
        [Category(DisplayCategory)]
        [Description("Set to false to use HeaderColors resource keys")]
        [DisplayName("Use tabular shared colors")]
        public bool HeaderUseTabularSharedColors { get; set; }

        [Category(DisplayCategory)]
        [Description("When to show the report total row")]
        [DisplayName("Report total row")]
        public ReportTotalRow ReportTotalRow { get; set; }
        #endregion

        #region source view category
        [Category(SourceViewCategory)]
        [Description("Use the full path for the name or just the directory name.")]
        [DisplayName("Root directory name show full path")]
        public bool RootDirectoryNameFromPath { get; set; }

        [Category(SourceViewCategory)]
        [Description("Controls the tree item nesting of a file when source view.")]
        [DisplayName("Source file structure")]
        public SourceFileStructure SourceFileStructure { get; set; }
        #endregion
    }
}
