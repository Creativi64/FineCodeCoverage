using System;
using System.Collections.Generic;

namespace FineCodeCoverage.Engine.ReportGenerator
{

    public enum VerbosityLevel
    {
        /// <summary>
        /// All messages are logged.
        /// </summary>
        Verbose,

        /// <summary>
        /// Only important messages are logged.
        /// </summary>
        Info,

        /// <summary>
        /// Only warnings and errors are logged.
        /// </summary>
        Warning,

        /// <summary>
        /// Only errors are logged.
        /// </summary>
        Error,

        /// <summary>
        /// Nothing is logged.
        /// </summary>
        Off
    }


    interface IFCCReportGenerator
    {
        void SetLogger(VerbosityLevel verbosityLevel, Action<VerbosityLevel, string> logger);
        IReportResult Generate(IEnumerable<string> coverageFiles, string reportDirectory, IEnumerable<string> reportTypes);
    }
}
