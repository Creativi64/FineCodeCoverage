namespace FineCodeCoverage.Core.Utilities
{
    public interface IShownReportToolWindowHistory
    {
        bool HasShownToolWindow { get; }

        void ShowedToolWindow();
    }
}
