using System;

namespace FineCodeCoverage.Core.Utilities
{
    internal sealed class ProcessResponseException : Exception
    {
        public ProcessResponseException(string message)
            : base(message)
        {
        }

        public ProcessResponseException()
            : base()
        {
        }

        public ProcessResponseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
