namespace FineCodeCoverage.Initialization.History
{
    public interface IShownReportToolWindowHistory
    {
        bool HasShownToolWindow { get; }

        void ShowedToolWindow();
    }
}
