namespace FineCodeCoverage.Core.Utilities
{
    interface IVsOpenFile
    {
        void OpenFileInCodeEditor(string path);
        void OpenFileInDefaultViewer(string path);
    }
}
