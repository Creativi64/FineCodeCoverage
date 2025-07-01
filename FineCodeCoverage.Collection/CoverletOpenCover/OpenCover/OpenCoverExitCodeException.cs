using System;

namespace FineCodeCoverage.Collection.CoverletOpenCover.OpenCover
{
    internal sealed class OpenCoverExitCodeException : Exception
    {
        public OpenCoverExitCodeException(string message)
            : base(message)
        {
        }

        public OpenCoverExitCodeException()
            : base()
        {
        }

        public OpenCoverExitCodeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
