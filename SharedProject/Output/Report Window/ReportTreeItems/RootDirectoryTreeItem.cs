using FineCodeCoverage.Options;

namespace FineCodeCoverage.Output
{
    internal class RootDirectoryTreeItem : DirectoryTreeItem
    {
        private readonly string name;
        private readonly string path;

        public RootDirectoryTreeItem(IDirectory directory, string path, bool nameIsPath, SourceFileStructure sourceFileStructure) : base(directory, sourceFileStructure)
        {
            this.name = Name;
            this.path = path;
            SetName(nameIsPath);
        }

        public void SetName(bool fromPath)
        {
            this.Name = fromPath ? path : name;
        }
    }
}
