namespace FineCodeCoverage.Core.Utilities
{
    internal interface IShownToolWindowHistory
    {
        bool HasShownToolWindow { get; }
        void ShowedToolWindow();
    }
}
