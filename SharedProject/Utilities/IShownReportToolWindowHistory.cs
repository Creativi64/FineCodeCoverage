namespace FineCodeCoverage.Core.Utilities
{
    internal interface IShownReportToolWindowHistory
    {
        bool HasShownToolWindow { get; }

        void ShowedToolWindow();
    }
}
