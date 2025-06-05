namespace FineCodeCoverage.Options.Report
{
    internal interface IIconOptions
    {
        bool ShowIcons { get; set; }

        int IconSize { get; set; }

        ThemedIconStyle ThemedIconStyle { get; set; }
    }
}
