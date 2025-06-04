namespace FineCodeCoverage.Readme
{
    internal class TemplatedReadmeInfo
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
