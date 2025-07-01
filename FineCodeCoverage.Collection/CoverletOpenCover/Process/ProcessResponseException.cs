using System;

namespace FineCodeCoverage.Collection.CoverletOpenCover.Process
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
