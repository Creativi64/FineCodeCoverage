namespace FineCodeCoverage.Core.Utilities
{
    public interface IVsOpenFile
    {
        void OpenFileInCodeEditor(string path);

        void OpenFileInDefaultViewer(string path);
    }
}
