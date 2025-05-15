using FineCodeCoverage.Output;

namespace FineCodeCoverage.Options
{
    /*
        Note that option properties must not be renamed
    */
    interface IEnabledOption
    {
        bool Enabled { get; set; }
    }

    internal interface IFCCCommonIncludesExcludes
    {

        bool IncludeTestAssembly { get; set; }
        bool IncludeReferencedProjects { get; set; }

        string[] ExcludeAssemblies { get; set; }
        string[] IncludeAssemblies { get; set; }
    }

    internal interface IMsCodeCoverageIncludesExcludesOptions
    {
        string[] ModulePathsExclude { get; set; }
        string[] ModulePathsInclude { get; set; }
        string[] CompanyNamesExclude { get; set; }
        string[] CompanyNamesInclude { get; set; }
        string[] PublicKeyTokensExclude { get; set; }
        string[] PublicKeyTokensInclude { get; set; }
        string[] SourcesExclude { get; set; }
        string[] SourcesInclude { get; set; }
        string[] AttributesExclude { get; set; }
        string[] AttributesInclude { get; set; }
        string[] FunctionsInclude { get; set; }
        string[] FunctionsExclude { get; set; }
    }
    internal interface IMsCodeCoverageOptions : IMsCodeCoverageIncludesExcludesOptions, IFCCCommonIncludesExcludes, IEnabledOption { }
    internal enum RunMsCodeCoverage { No, IfInRunSettings, Yes }

    internal interface IOpenCoverCoverletExcludeIncludeOptions
    {
        string[] Exclude { get; set; }
        string[] ExcludeByAttribute { get; set; }
        string[] ExcludeByFile { get; set; }
        string[] Include { get; set; }
    }

    internal enum OpenCoverRegister { Default,NoArg, User, Path32, Path64}

    internal interface IOpenCoverOptions
    {
        string OpenCoverCustomPath { get; set; }
        OpenCoverRegister OpenCoverRegister { get; set; }
        string OpenCoverTarget { get; set; }
        string OpenCoverTargetArgs { get; set; }
    }

    interface IOverviewMarginOptions
    {
        bool ShowCoverageInOverviewMargin { get; set; }
        bool ShowCoveredInOverviewMargin { get; set; }
        bool ShowUncoveredInOverviewMargin { get; set; }
        bool ShowPartiallyCoveredInOverviewMargin { get; set; }
        bool ShowDirtyInOverviewMargin { get; set; }
        bool ShowNewInOverviewMargin { get; set; }
        bool ShowNotIncludedInOverviewMargin { get; set; }
    }

    interface IGlyphMarginOptions
    {
        bool ShowCoverageInGlyphMargin { get; set; }
        bool ShowCoveredInGlyphMargin { get; set; }
        bool ShowUncoveredInGlyphMargin { get; set; }
        bool ShowPartiallyCoveredInGlyphMargin { get; set; }
        bool ShowDirtyInGlyphMargin { get; set; }
        bool ShowNewInGlyphMargin { get; set; }
        bool ShowNotIncludedInGlyphMargin { get; set; }
    }

    interface IEditorLineHighlightingCoverageOptions
    {
        bool ShowLineCoverageHighlighting { get; set; }
        bool ShowLineCoveredHighlighting { get; set; }
        bool ShowLineUncoveredHighlighting { get; set; }
        bool ShowLinePartiallyCoveredHighlighting { get; set; }
        bool ShowLineDirtyHighlighting { get; set; }
        bool ShowLineNewHighlighting { get; set; }
        bool ShowLineNotIncludedHighlighting { get; set; }
    }

    internal enum EditorCoverageColouringMode
    {
        UseRoslynWhenTextChanges,
        DoNotUseRoslynWhenTextChanges,
        Off
    }

    interface IEditorCoverageColouringOptions : IOverviewMarginOptions, IGlyphMarginOptions,IEditorLineHighlightingCoverageOptions {
        bool ShowEditorCoverage { get; set; }
        bool UseEnterpriseFontsAndColors { get; set; }
        EditorCoverageColouringMode EditorCoverageColouringMode { get; set; }
    }

    internal interface ICoveragePercentageBarOptions
    {
        bool CoveragePercentageUseSolidBrush { get; set; }
        bool CoveragePercentageCoveredIsLeft { get; set; }
        CoveragePercentageBarDisplayParts CoveragePercentageDisplayParts { get; set; }
        bool CoveragePercentageIsThemed { get; set; }
        bool CoveragePercentageUseColorsFromFontsAndColors { get; set; }
        bool CoveragePercentageUseContrastedThemeWhenSingularDisplay { get; set; }
        double? CoveragePercentageHeightOrMultiplier { get; set; }
        bool CoveragePercentageShowTooltip { get; set; }
    }

    internal enum ThemedIconStyle { MonochromeGlyph, MonochromeText, Moniker}
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

    internal interface IReportDisplayOptions : ICoveragePercentageBarOptions, IIconOptions {
        // https://learn.microsoft.com/en-us/visualstudio/extensibility/ux-guidelines/shared-colors-for-visual-studio?view=vs-2022#tabular-data-grid-controls
        bool HeaderUseTabularSharedColors { get; set; }
        ReportTotalRow ReportTotalRow { get; set; }
        bool RootDirectoryNameFromPath { get; set; }
        SourceFileStructure SourceFileStructure { get; set; }
    }

    internal interface ICoverletOptions
    {
        bool CoverletConsoleGlobal { get; set; }
        string CoverletConsoleCustomPath { get; set; }
        bool CoverletConsoleLocal { get; set; }
        string CoverletCollectorDirectoryPath { get; set; }
        bool RunSettingsOnly { get; set; }
    }

    interface IHotspotThresholds
    {
        int ThresholdForCyclomaticComplexity { get; set; }
        int ThresholdForNPathComplexity { get; set; }
        int ThresholdForCrapScore { get; set; }
    }

    interface IBlazorOptions
    {
        bool BlazorCoverageLinesFromGeneratedSource { get; set; }
    }

    interface IReportFilters {
        bool HideFullyCovered { get; set; }
        bool Hide0Coverable { get; set; }
        bool Hide0Coverage { get; set; }
    }


    interface IRunOptions : IEnabledOption
    {
        bool DisabledNoCoverage { get; set; }
        bool RunInParallel { get; set; }
        int RunWhenTestsExceed { get; set; }
        bool RunWhenTestsFail { get; set; }
        RunMsCodeCoverage RunMsCodeCoverage { get; set; }
    }
    internal interface IInstallationOptions
    {
        string ToolsDirectory { get; set; }
    }
    internal interface IOpenCoverCoverletBuildOutputOptions
    {
        bool AdjacentBuildOutput { get; set; }
    }

    internal interface ICommonOutputOptions
    {
        string FCCSolutionOutputDirectoryName { get; set; }
    }

    internal interface IOutputOptions : IOpenCoverCoverletBuildOutputOptions, ICommonOutputOptions { }

    internal interface IAppOptions :
        IMsCodeCoverageIncludesExcludesOptions,
        IOpenCoverCoverletExcludeIncludeOptions,
        IFCCCommonIncludesExcludes,
        IOpenCoverOptions,
        ICoverletOptions,
        //IEditorCoverageColouringOptions,
        //IBlazorOptions,
        IReportDisplayOptions,
        IHotspotThresholds,
        IReportFilters,
        IRunOptions,
        IInstallationOptions,
        IOutputOptions
    { }
}