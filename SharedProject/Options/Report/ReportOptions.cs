using System.ComponentModel;

namespace FineCodeCoverage.Options
{
    internal enum ThemedIconStyle { MonochromeGlyph, MonochromeText, Moniker }

    interface IIconOptions
    {
        bool ShowIcons { get; set; }
        int IconSize { get; set; }
        ThemedIconStyle ThemedIconStyle { get; set; }
    }

    internal enum ReportTotalRow
    {
        WhenRequired,
        Always,
        Never
    }

    internal enum SourceFileStructure
    {
        Method,
        Class,
        NamespaceAndClass,
        AsRequired
    }

    /*
        Note that option properties must not be renamed
    */
    internal class ReportOptions
    {
        private const string filtersCategory = "Filters";
        private const string iconCategory = "Icons";
        private const string displayCategory = "Display";
        private const string coverageBarCategory = "Coverage Bar";
        private const string sourceViewCategory = "Source View";

        #region filters category
        [Category(filtersCategory)]
        [Description("Set to true to hide classes, namespaces and assemblies that are fully covered.")]
        [DisplayName("Hide Fully Covered")]
        public bool HideFullyCovered { get; set; }

        [Category(filtersCategory)]
        [Description("Set to false to show classes, namespaces and assemblies that are not coverable.")]
        [DisplayName("Hide Not Coverable")]
        public bool Hide0Coverable { get; set; }

        [Category(filtersCategory)]
        [Description("Set to true to hide classes, namespaces and assemblies that have 0% coverage.")]
        [DisplayName("Hide 0% Coverage")]
        public bool Hide0Coverage { get; set; }
        #endregion

        #region icons category
        [Category(iconCategory)]
        [Description("Set to false to not display tree item icons")]
        [DisplayName("Show icons")]
        public bool ShowIcons { get; set; }

        [Category(iconCategory)]
        [Description("Set the icon size")]
        [DisplayName("Icon size")]
        public int IconSize { get; set; }

        [Category(iconCategory)]
        [Description("Themed icon style")]
        [DisplayName("Themed Style")]
        public ThemedIconStyle ThemedIconStyle { get; set; }
        #endregion

        #region coverage bar category
        [Category(coverageBarCategory)]
        [Description("Covered on the left or the right")]
        [DisplayName("Covered is left")]
        public bool CoveragePercentageCoveredIsLeft { get; set; }

        [Category(coverageBarCategory)]
        [Description("Display covered and uncovered percentages ( Both ), Covered or UnCovered")]
        [DisplayName("Display parts")]
        public CoveragePercentageBarDisplayParts CoveragePercentageDisplayParts { get; set; }

        [Category(coverageBarCategory)]
        [Description("Theme colours against the background.")]
        [DisplayName("Is themed")]
        public bool CoveragePercentageIsThemed { get; set; }

        [Category(coverageBarCategory)]
        [Description("Use colours from Environment / Fonts and Colors - e.g Coverage Touched Area FCC")]
        [DisplayName("Use fonts and colors")]
        public bool CoveragePercentageUseColorsFromFontsAndColors { get; set; }

        [Category(coverageBarCategory)]
        [Description("Set to false for dashed line brush")]
        [DisplayName("Use solid brush")]
        public bool CoveragePercentageUseSolidBrush { get; set; }

        [Category(coverageBarCategory)]
        [Description("Use contrast color when singular display")]
        [DisplayName("Use contrast color when singular display")]
        public bool CoveragePercentageUseContrastedThemeWhenSingularDisplay { get; set; }

        [Category(coverageBarCategory)]
        [Description("Set to between 0 and 1 as percentage of environment font size or a specific height.")]
        [DisplayName("Height in pixels or multiplier")]
        public double? CoveragePercentageHeightOrMultiplier { get; set; }

        [Category(coverageBarCategory)]
        [Description("Set to false to hide tooltip showing contributing amounts.")]
        [DisplayName("Show tooltip")]
        public bool CoveragePercentageShowTooltip { get; set; }
        #endregion

        #region display category
        [Category(displayCategory)]
        [Description("Set to false to use HeaderColors resource keys")]
        [DisplayName("Use tabular shared colors")]
        public bool HeaderUseTabularSharedColors { get; set; }

        [Category(displayCategory)]
        [Description("When to show the report total row")]
        [DisplayName("Report total row")]
        public ReportTotalRow ReportTotalRow { get; set; }
        #endregion

        #region source view category
        [Category(sourceViewCategory)]
        [Description("Use the full path for the name or just the directory name.")]
        [DisplayName("Root directory name show full path")]
        public bool RootDirectoryNameFromPath { get; set; }

        [Category(sourceViewCategory)]
        [Description("Controls the tree item nesting of a file when source view.")]
        [DisplayName("Source file structure")]
        public SourceFileStructure SourceFileStructure { get; set; }
        #endregion
    }
}
