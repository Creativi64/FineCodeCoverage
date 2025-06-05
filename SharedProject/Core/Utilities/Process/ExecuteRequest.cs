namespace FineCodeCoverage.Core.Utilities
{
    internal class ExecuteRequest
    {
        public string FilePath { get; set; }

        public string Arguments { get; set; }

        public string WorkingDirectory { get; set; }
    }
}
