namespace FineCodeCoverage.VSAbstractions.Files
{
    public interface IVsOpenFile
    {
        void OpenFileInCodeEditor(string path);

        void OpenFileInDefaultViewer(string path);
    }
}
