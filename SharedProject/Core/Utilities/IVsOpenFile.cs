namespace FineCodeCoverage.Core.Utilities
{
    internal interface IVsOpenFile
    {
        void OpenFileInCodeEditor(string path);
        void OpenFileInDefaultViewer(string path);
    }
}
