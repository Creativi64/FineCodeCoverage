using System;

namespace FineCodeCoverage.Core.Utilities
{
    internal class ExecuteResponse
    {
        public int ExitCode { get; set; }
        public DateTimeOffset ExitTime { get; set; }
        public TimeSpan RunTime { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public string Output { get; set; }
    }
}