namespace FineCodeCoverage.Core.Utilities
{
    internal sealed class ExecuteRequest
    {
        public string FilePath { get; set; }

        public string Arguments { get; set; }

        public string WorkingDirectory { get; set; }
    }
}
