using System;
using System.Collections.Generic;

namespace FineCodeCoverage.Engine.Model
{
    internal sealed class CoverageProjectFileSynchronizationDetails
    {
        public List<string> Logs { get; set; } = new List<string>();

        public TimeSpan Duration { get; set; }
    }
}
