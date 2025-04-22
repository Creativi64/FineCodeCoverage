namespace FineCodeCoverage.Core.Utilities
{
    internal class FileRename
    {
        public FileRename(string oldFilePath, string newFilePath)
        {
            OldFilePath = oldFilePath;
            NewFilePath = newFilePath;
        }

        public string OldFilePath { get; }
        public string NewFilePath { get; }
    }
}
