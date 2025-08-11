namespace FineCodeCoverage.Readme.Template
{
    public sealed class TemplatedReadmeInfo
    {
        public TemplatedReadmeInfo(string readme, string directory)
        {
            Readme = readme;
            Directory = directory;
        }

        public string Readme { get; }

        public string Directory { get; }
    }
}
