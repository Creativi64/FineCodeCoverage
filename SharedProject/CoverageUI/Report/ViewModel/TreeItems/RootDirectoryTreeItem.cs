using FineCodeCoverage.Options.Report;

namespace FineCodeCoverage.Output
{
    internal sealed class RootDirectoryTreeItem : DirectoryTreeItem
    {
        private readonly string _name;
        private readonly string _path;

        public RootDirectoryTreeItem(
            IDirectory directory,
            string path,
            bool nameIsPath,
            SourceFileStructure sourceFileStructure,
            IChangeset changeset = null)
            : base(directory, sourceFileStructure, changeset)
        {
            _name = Name;
            _path = path;
            SetName(nameIsPath);
        }

        public void SetName(bool fromPath) => Name = fromPath ? _path : _name;
    }
}
