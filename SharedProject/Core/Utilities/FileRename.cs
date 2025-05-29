namespace FineCodeCoverage.Core.Utilities
{
    internal class FileRename
    {
        public FileRename(string oldFilePath, string newFilePath)
        {
            this.OldFilePath = oldFilePath;
            this.NewFilePath = newFilePath;
        }

        public string OldFilePath { get; }
        public string NewFilePath { get; }
    }
}
