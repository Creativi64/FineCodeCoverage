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
            this._name = this.Name;
            this._path = path;
            this.SetName(nameIsPath);
        }

        public void SetName(bool fromPath) => this.Name = fromPath ? this._path : this._name;
    }
}