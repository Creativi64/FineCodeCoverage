namespace FineCodeCoverage.Readme
{
    internal class TemplatedReadmeInfo
    {
        public TemplatedReadmeInfo(string readme, string directory)
        {
            this.Readme = readme;
            this.Directory = directory;
        }

        public string Readme { get; }
        public string Directory { get; }
    }
}
