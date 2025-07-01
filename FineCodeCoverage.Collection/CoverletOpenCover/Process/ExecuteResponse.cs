using System;

namespace FineCodeCoverage.Collection.CoverletOpenCover.Process
{
    internal sealed class ExecuteResponse
    {
        public int ExitCode { get; set; }

        public DateTimeOffset ExitTime { get; set; }

        public TimeSpan RunTime { get; set; }

        public DateTimeOffset StartTime { get; set; }

        public string Output { get; set; }
    }
}
