namespace FineCodeCoverage.Options.Report
{
    interface IIconOptions
    {
        bool ShowIcons { get; set; }
        int IconSize { get; set; }
        ThemedIconStyle ThemedIconStyle { get; set; }
    }
}
