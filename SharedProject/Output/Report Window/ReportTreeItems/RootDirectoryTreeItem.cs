using FineCodeCoverage.Options;

namespace FineCodeCoverage.Output
{
    internal class RootDirectoryTreeItem : DirectoryTreeItem
    {
        private readonly string _name;
        private readonly string _path;

        public RootDirectoryTreeItem(
            IDirectory directory,
            string path,
            bool nameIsPath,
            SourceFileStructure sourceFileStructure
        ) : base(directory, sourceFileStructure)
        {
            _name = Name;
            _path = path;
            SetName(nameIsPath);
        }

        public void SetName(bool fromPath) => Name = fromPath ? _path : _name;
    }
}
